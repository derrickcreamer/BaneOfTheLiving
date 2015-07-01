//
using System;
using System.Collections.Generic;
namespace Attributes{
	//todo: put some explanation here about how these are supposed to work.
	//Note that "value = x" or "value++", etc., are saying "find the sourceless non-expiring value and update it, or add it if it doesn't exist."
	public class Attribute<T>{
		public T type;
		public int value;
		public bool dead = false;
		public bool never_expires = false;
		public IComparable expiration_time;
		public object source;
		public Attribute(T type_,int value_,object source_,IComparable expiration_time_){
			type = type_;
			value = value_;
			source = source_;
			expiration_time = expiration_time_;
		}
		public Attribute(T type_,int value_,object source_){
			type = type_;
			value = value_;
			source = source_;
			never_expires = true;
		}
		public static Attribute<T> Create(T type_,int value_,object source_,IComparable expiration_time_){
			return new Attribute<T>(type_,value_,source_,expiration_time_);
		}
		public static Attribute<T> Create(T type_,int value_,object source_){
			return new Attribute<T>(type_,value_,source_);
		}
	}
	public class AttributeDict<T>{
		public Dictionary<T,List<Attribute<T>>> d = new Dictionary<T,List<Attribute<T>>>();
		public static Func<IComparable> get_time;
		public static bool expires_at_exact_time = false; //By default, expiration happens AFTER the given time. If you want it to happen AT the given time, use this.
		public AttributeDict(){}
		public int this[T attr]{
			get{
				if(!d.ContainsKey(attr)){
					return 0;
				}
				int total = 0;
				IComparable current_time = get_time();
				for(int i=0;i<d[attr].Count;++i){
					Attribute<T> a = d[attr][i];
					if(a.dead){
						d[attr].RemoveAt(i);
						--i;
					}
					else{
						if(a.never_expires){
							total += a.value;
						}
						else{
							int comparison = current_time.CompareTo(a.expiration_time);
							if(comparison < 0){
								total += a.value;
							}
							else{
								if(comparison > 0){
									d[attr].RemoveAt(i);
									--i;
								}
								else{
									if(expires_at_exact_time){
										d[attr].RemoveAt(i);
										--i;
									}
									else{
										total += a.value;
									}
								}
							}
						}
					}
				}
				return total;
			}
			set{
				if(value == 0){
					if(d.ContainsKey(attr)){
						d[attr].Clear();
					}
				}
				else{
					if(!d.ContainsKey(attr)){
						d.Add(attr,new List<Attribute<T>>{new Attribute<T>(attr,value,null)});
					}
					else{
						if(d[attr].Count == 1 && d[attr][0].never_expires && Object.Equals(d[attr][0].source,null)){
							d[attr][0].value = value; //If there's already a single non-expiring sourceless attribute, simply update its value rather than create a new list.
						}
						else{
							d[attr] = new List<Attribute<T>>{new Attribute<T>(attr,value,null)}; //if the user sets the value directly, assume they want that as the ONLY value. e.g. attrs[Attr.Enraged] = 1;
						}
					}
				}
			}
		}
		public int this[T attr,object source]{
			get{
				if(!d.ContainsKey(attr)){
					return 0;
				}
				int total = 0;
				IComparable current_time = get_time();
				for(int i=0;i<d[attr].Count;++i){
					Attribute<T> a = d[attr][i];
					if(Object.Equals(a.source,source)){
						if(a.dead){
							d[attr].RemoveAt(i);
							--i;
						}
						else{
							if(a.never_expires){
								total += a.value;
							}
							else{
								int comparison = current_time.CompareTo(a.expiration_time);
								if(comparison < 0){
									total += a.value;
								}
								else{
									if(comparison > 0){
										d[attr].RemoveAt(i); //because source is checked first, this does mean that expired attrs aren't discarded if they don't match the source.
										--i;
									}
									else{
										if(expires_at_exact_time){
											d[attr].RemoveAt(i);
											--i;
										}
										else{
											total += a.value;
										}
									}
								}
							}
						}
					}
				}
				return total;
			}
			set{
				if(!d.ContainsKey(attr)){
					if(value != 0){
						d.Add(attr,new List<Attribute<T>>{new Attribute<T>(attr,value,source)});
					}
				}
				else{ //note: if you keep a reference to an Attribute<T>, its behavior is NOT guaranteed. In this case, it's because I use the same one and update its values.
					bool placed = (value == 0); // false by default, but if value is 0, delete them all anyway.
					for(int i=0;i<d[attr].Count;++i){
						Attribute<T> a = d[attr][i];
						if(Object.Equals(a.source,source)){
							if(!a.dead && a.never_expires && !placed){
								placed = true;
								a.value = value;
							}
							else{
								d[attr].RemoveAt(i);
								--i;
							}
						}
					}
					if(!placed){
						d[attr].Add(new Attribute<T>(attr,value,source));
					}
				}
			}
		}
		public int Total(T attr){
			return this[attr];
		}
		public bool Has(T attr){
			return this[attr] > 0;
		}
		public int Highest(T attr){
			if(!d.ContainsKey(attr)){
				return 0;
			}
			int highest = 0;
			IComparable current_time = get_time();
			for(int i=0;i<d[attr].Count;++i){
				Attribute<T> a = d[attr][i];
				if(a.dead){
					d[attr].RemoveAt(i);
					--i;
				}
				else{
					if(a.never_expires){
						if(a.value > highest){
							highest = a.value;
						}
					}
					else{
						int comparison = current_time.CompareTo(a.expiration_time);
						if(comparison < 0){
							if(a.value > highest){
								highest = a.value;
							}
						}
						else{
							if(comparison > 0){
								d[attr].RemoveAt(i);
								--i;
							}
							else{
								if(expires_at_exact_time){
									d[attr].RemoveAt(i);
									--i;
								}
								else{
									if(a.value > highest){
										highest = a.value;
									}
								}
							}
						}
					}
				}
			}
			return highest;
		}
		public int TotalFromSource(T attr,object source){
			return this[attr,source];
		}
		public bool HasFromSource(T attr,object source){
			return this[attr,source] > 0;
		}
		public int HighestFromSource(T attr,object source){
			if(!d.ContainsKey(attr)){
				return 0;
			}
			int highest = 0;
			IComparable current_time = get_time();
			for(int i=0;i<d[attr].Count;++i){
				Attribute<T> a = d[attr][i];
				if(Object.Equals(a.source,source)){
					if(a.dead){
						d[attr].RemoveAt(i);
						--i;
					}
					else{
						if(a.never_expires){
							if(a.value > highest){
								highest = a.value;
							}
						}
						else{
							int comparison = current_time.CompareTo(a.expiration_time);
							if(comparison < 0){
								if(a.value > highest){
									highest = a.value;
								}
							}
							else{
								if(comparison > 0){
									d[attr].RemoveAt(i);
									--i;
								}
								else{
									if(expires_at_exact_time){
										d[attr].RemoveAt(i);
										--i;
									}
									else{
										if(a.value > highest){
											highest = a.value;
										}
									}
								}
							}
						}
					}
				}
			}
			return highest;
		}
		public static AttributeDict<T> operator +(AttributeDict<T> d,Attribute<T> attr){
			if(d.d.ContainsKey(attr.type)){
				d.d[attr.type].Add(attr);
			}
			else{
				d.d.Add(attr.type,new List<Attribute<T>>{attr});
			}
			return d;
		}
		public List<Attribute<T>> GetValueList(T attr){
			if(d.ContainsKey(attr)){
				return d[attr];
			}
			return new List<Attribute<T>>();
		}
		/*public Attribute<T> Add(T attr,int val,object src,IComparable expires){
			if(!d.ContainsKey(attr)){
				d.Add(attr,new List<Attribute<T>>());
			}
			var a = new Attribute<T>(attr,val,src,expires);
			d[attr].Add(a);
			return a;
		}
		public Attribute<T> Add(T attr,object src,IComparable expires){
			return Add(attr,1,src,expires);
		}
		public Attribute<T> Add(T attr,IComparable expires){
			return Add(attr,1,null,expires);
		}*/
	}
}
