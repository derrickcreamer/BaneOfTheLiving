//
using System;
//using System.Collections.Generic;
//using Utilities;
using Utilities;
using Bane;
namespace Bane{
	public enum Skill{Distract,Dodge,Dagger,Shadow_Sneak,Climber,Pursuit,Swap,Front_Shield,Defend_Ally,Block,Longsword,Reposition,Crippling_Strike,Fog_Vision,Fast,Mark_Target,Scout_Ahead,Alarm,Bow,Sharp_Ears,Slowing,Mend,Invigorate,Push,Calming_Voice,Warning,Hammer,Lay_Down_Arms,Charge,Retaliate,Hunger,Bring_Low,Maul,Taunt,Hurry,Metabolize_Magic,Hold,Staff_of_Wind,Zap,Mana_Burn,Chill,Arcane_Babble,Reassemble,Slow,Tough,Grab,Mummy_Rot,Absorb_Essence,Possess,Fade,Incorporeal,Fragile,Shamble,Move,UnarmedAttack}; //Move and UnarmedAttack are special cases, available to all units
	public static class Skills{
		private static int[] category = new int[]{2,2,1,4,0,2,3,1,3,2,1,3,2,4,0,2,2,3,1,0,4,3,3,2,3,3,1,4,2,2,0,4,1,2,3,0,4,1,4,4,4,3,0,0,0,2,0,2,4,0,0,0,0,-1,-1};
		private static string[] name = new string[]{"Distract","Dodge","Dagger","Shadow Sneak","Climber","Pursuit","Swap","Front Shield","Defend Ally","Block","Longsword","Reposition","Crippling Strike","Fog Vision","Fast","Mark Target","Scout Ahead","Alarm","Bow","Sharp Ears","Slowing","Mend","Invigorate","Push","Calming Voice","Warning","Hammer","Lay Down Arms","Charge","Retaliate","Hunger","Bring Low","Maul","Taunt","Hurry","Metabolize Magic","Hold","Staff of Wind","Zap","Mana Burn","Chill","Arcane Babble","Reassemble","Slow","Tough","Grab","Mummy Rot","Absorb Essence","Possess","Fade","Incorporeal","Fragile","Shamble","Double Move","Unarmed Attack"};
		public static bool[] active = new bool[]{true,true,true,true,false,false,true,false,true,false,true,true,false,true,false,true,false,true,true,false,true,true,true,true,true,true,true,true,true,false,false,true,true,true,true,false,true,true,true,true,true,true,false,false,false,false,false,false,true,false,false,false,false,true,true};
		public static int Category(this Skill s){ return category[(int)s]; } //todo: Later, I might make proper methods of these, with switches and stuff.
		public static string Name(this Skill s){ return name[(int)s]; }
		public static bool Active(this Skill s){ return active[(int)s]; } //todo: okay, DEFINITELY change this one. yikes.
		public static bool IsAttack(this Skill s){
			switch(s){
			case Skill.Bow:
			case Skill.Bring_Low:
			case Skill.Charge:
			case Skill.Chill:
			case Skill.Dagger:
			case Skill.Hammer:
			case Skill.Longsword:
			case Skill.Mana_Burn:
			case Skill.Maul:
			case Skill.Zap:
			case Skill.UnarmedAttack:
			return true;
			}
			return false;
		}
		/*public static TargetingRule FirstTarget(this Skill s){
			switch(s){
			case Skill.Move:
			return TargetingRule.AnyTile; //todo!
			case Skill.Alarm:
			case Skill.Arcane_Babble:
			case Skill.Calming_Voice:
			case Skill.Defend_Ally:
			case Skill.Hurry:
			case Skill.Invigorate:
			case Skill.Mend:
			case Skill.Reposition:
			case Skill.Swap:
			case Skill.Warning:
			return TargetingRule.Ally;
			case Skill.Shadow_Sneak:
			{
				UI.ActorCheck has_space_behind = a => {
					Tile t = a.TileInDirection(a.FacingDirection().RotateFourWayDir(true,2));
					if(t == null){
						return false;
					}
					if(t.Ac == null){
						return true;
					}
					return false;
				};
				UI.TileCheck is_behind_enemy = x => {
					if(x.Ac != null){
						return false;
					}
					foreach(int dir in U.FourDirections){
						if(x.ActorInDirection(dir) != null && x.ActorInDirection(dir).IsEnemy(this) && x.ActorInDirection(dir).FacingDirection() == dir){
							return true;
						}
					}
					return false;
				};
				return apply_skill(range => UI.GetTarget(p,range,is_behind_enemy,has_space_behind,is_behind_enemy),8,(t,t2)=>{
					Move(t);
					return true;
				});
			}
			case Skill.Lay_Down_Arms:
			foreach(Actor a in ActorsWithinDistance(4)){
				Actor ac = a;
				ac += UntilStartOfAction(Attr.Disarmed.Ready(this));
			}
			return true;
			case Skill.Push:
			return apply_skill(GetEnemy,1,(t,t2)=>{
				Tile dest = t.TileInDirection(this.DirectionOf(t));
				if(dest != null){
					t.Ac.Move(dest); //todo checks etc. Also does this push farther if they're vulnerable?
					return true;
				}
				return false;
			});
			case Skill.Slowing:
			return apply_skill(GetTile,4,(t,t2)=>{
				foreach(Actor a in t.ActorsWithinDistance(1)){
					Actor ac = a;
					ac += this.UntilStartOfAction(Attr.BonusMovement.Ready(-2,this));
				}
				return true;
			});
			case Skill.Staff_of_Wind:
			return apply_skill(GetAllyOrEnemy,4,t=>t.GetTile,1,(t,t2)=>{
				t.Ac.Move(t2); //todo: restrictions?
				return true;
			});
			case Skill.Zap:
			return apply_skill(GetTile,1,(t,t2)=>{ //targeting can be improved here to show affected tiles, todo. this one is a beam.
				if(DistanceFrom(t) == 0){
					return false;
				}
				int dir = DirectionOf(t);
				while(t != null && DistanceFrom(t) <= 6){
					if(t.Ac != null){
						t.Ac.TakeDamage(2,this);
					}
					t = t.TileInDirection(dir);
				}
				return true;
			});
			case Skill.Charge:
			return apply_skill(GetTile,1,(t,t2)=>{ //todo: this is a skill that would have different parameters for what's displayed while targeting vs. what's accepted while targeting.
				if(DistanceFrom(t) == 0 || t.Ac != null || t.ElevationDifference(Ti) > 1){ //todo: check for movement restrictions, right?
					return false;
				}
				int dir = DirectionOf(t); //todo: or maybe this skill should work like d&d charge - can't move first, but you move double and attack. (bonus damage only after X tiles?)
				for(int i=0;i<3;++i){
					Move(t.p);
					Tile next = t.TileInDirection(dir);
					if(next == null || next.Ac != null || next.ElevationDifference(t) > 1){
						break;
					}
					t = next;
				}
				if(ActorInDirection(dir) != null && IsEnemy(ActorInDirection(dir))){
					Actor ac = ActorInDirection(dir);
					int dmg = 2; //this is just Maul. todo.
					if(ac.health == ac.max_health){
						dmg++;
					}
					ac.TakeDamage(dmg,this);
				}
				return true;
			});
			case Skill.Dodge:
			return false; //todo remove, remake, or reconsider
			default:
			return TargetingRule.Enemy;
			}
		}*/
		public static int FirstRange(this Skill s){
			switch(s){
			default:
			return 6;
			}
		}
		public static TargetingRule SecondTarget(this Skill s){
			switch(s){
			default:
			return TargetingRule.None;
			}
		}
		public static int SecondRange(this Skill s){
			switch(s){
			default:
			return 6;
			}
		}
		/*public static Actor.TargetResult Effect(this Skill s){
			switch(s){
			default:
			return (t,t2)=>{};
			}
		}*/
	} // Body(0) - Equipment(1) - Warfare(2) - Support(3) - Magic(4) - Holy(5)
				//new Skill("Possess",true,4), //magic? warfare?
}

