//
using System;
using System.Collections.Generic;
using Utilities;
namespace Bane{
	public static class Q{
		public static LinkedList<Event> list;
		public static Event current_event = null;
		public static Time now{ get{ return current_event.time; } }
		public static Time next{ get{ return new Time(current_event.time.turn + 1,current_event.time.initiative_count); } }
		public static void Pop(){
			current_event = list.First.Value;
			current_event.Execute();
			list.Remove(current_event);
		}
	}
	public struct Time : IComparable, IComparable<Time>{
		public int turn;
		public int initiative_count;
		public Time(int turn_,int init){
			turn = turn_;
			initiative_count = init;
		}
		public Time next{ get{ return new Time(turn + 1,initiative_count); } }
		public Time start{ get{ return new Time(turn,-1); } } //todo: this currently happens before anything else on this turn. is this correct?
		public Time end{ get{ return new Time(turn,21); } }
		public int CompareTo(object obj){
			if(!(obj is Time)){
				throw new ArgumentException("Argument obj is not a Time");
			}
			return CompareTo((Time)obj);
		}
		public int CompareTo(Time other){
			int i = turn.CompareTo(other.turn);
			if(i == 0){
				return -(initiative_count.CompareTo(other.initiative_count));
			}
			return i;
		}
	}
	public enum EventType{Action,NewTurn};
	public class Event{
		public EventType type;
		public Time time;
		public List<Actor> actors = null;
		public Event(EventType type_,int turn,int init){
			type = type_;
			time = new Time(turn,init);
		}
		public Event(EventType type_,Time time_){
			type = type_;
			time = time_;
		}
		public void Execute(){
			switch(type){
			case EventType.Action:
			{
				while(actors.Count > 0){
					Actor a = actors[0];
					G.active_team = a.base_unit.team; //todo: what should I do with this?
					M.MoveToShow(a);
					if(a.team == 0){
						a.highlighted = true; //todo, is there a better color?
					}
					G.undead.Update();
					G.living.Update();
					a.Act();
					/*if(G.active_team == 0){
						a.SelectAction();
					}
					else{
						a.AI_SelectAction();
					}*/
					actors.Remove(a);
					a.highlighted = false;
				}
				break;
			}
			case EventType.NewTurn:
			{
				G.active_team = -1;
				//end of turn effects, like removal of stun
				//
				//
				List<Actor>[] bracket = new List<Actor>[21];
				List<Actor>[] teams = new List<Actor>[2];
				if(G.player_has_initiative_advantage){
					teams[0] = G.todo_team;
					teams[1] = G.foes;
				}
				else{
					teams[0] = G.foes;
					teams[1] = G.todo_team;
				}
				foreach(List<Actor> l in teams){
					foreach(Actor a in l){
						int init = a.initiative;
						if(bracket[init] == null){
							bracket[init] = new List<Actor>();
						}
						bracket[init].Add(a);
					}
				}
				Q.list = new LinkedList<Event>();
				for(int i=0;i<21;++i){
					if(bracket[i] != null){
						Event e = new Event(EventType.Action,time.turn + 1,i);
						e.actors = bracket[i];
						Q.list.AddFirst(e);
					}
				}
				Q.list.AddLast(new Event(EventType.NewTurn,time.next));
				break;
			}
			}
		}
	}
}
