//
using System;
using System.Collections.Generic;
using Utilities;
namespace Bane{
	//public enum UndeadType{Zombie,Skeleton,Spirit,Wraith,Cinder,Ghoul,Wight,Spectre,Mummy,Drowned,Banshee,Damned,Husk,Shade,COUNT,None};
	//public enum Job{Rogue,Champion,Scout,Healer,Soldier,Wizard,Necromancer,COUNT,None};
	public class Job{
		public int idx{get; private set;} //todo: replace these with enums too?
		public string name;
		public int initiative;
		public List<Skill> skills;
		private Job(string name_,int init,params Skill[] s){
			idx = next_idx++;
			name = name_;
			initiative = init;
			skills = new List<Skill>();
			foreach(Skill skill in s){
				skills.Add(skill);
			}
		}
		private static int next_idx = 0;
		private static Job[] j;
		static Job(){
			j = new Job[]{
				new Job("Rogue",15,Skill.Distract,Skill.Dodge,Skill.Dagger,Skill.Shadow_Sneak,Skill.Climber,Skill.Pursuit,Skill.Swap),
				new Job("Champion",10,Skill.Front_Shield,Skill.Defend_Ally,Skill.Block,Skill.Longsword,Skill.Reposition,Skill.Crippling_Strike,Skill.Fog_Vision),
				new Job("Scout",13,Skill.Fast,Skill.Mark_Target,Skill.Scout_Ahead,Skill.Alarm,Skill.Bow,Skill.Sharp_Ears,Skill.Slowing),
				new Job("Healer",8,Skill.Mend,Skill.Invigorate,Skill.Push,Skill.Calming_Voice,Skill.Warning,Skill.Hammer,Skill.Lay_Down_Arms),
				new Job("Soldier",11,Skill.Charge,Skill.Retaliate,Skill.Hunger,Skill.Bring_Low,Skill.Dodge,Skill.Maul,Skill.Taunt,Skill.Hurry),
				new Job("Wizard",7,Skill.Metabolize_Magic,Skill.Hold,Skill.Staff_of_Wind,Skill.Zap,Skill.Alarm,Skill.Mana_Burn,Skill.Chill,Skill.Arcane_Babble),
				new Job("Commoner",10),
			};
		}
		public static Job GetIndexed(int idx){ return j[idx]; }
		public static Job Rogue{get{ return j[0]; }}
		public static Job Champion{get{ return j[1]; }}
		public static Job Scout{get{ return j[2]; }}
		public static Job Healer{get{ return j[3]; }}
		public static Job Soldier{get{ return j[4]; }}
		public static Job Wizard{get{ return j[5]; }}
		public static Job Commoner{get{ return j[6]; }}
		public static Job Random{get{ return j[R.Between(0,5)]; }}
	}
	public class Species{
		public int idx{get; private set;}
		public string name;
		public int initiative_mod;
		public List<int> blocked_skill_categories;
		public List<Skill> skills;
		private Species(string name_,int init_mod,List<int> blocked_categories,params Skill[] sk){
			idx = next_idx++;
			name = name_;
			initiative_mod = init_mod;
			blocked_skill_categories = blocked_categories;
			skills = new List<Skill>();
			foreach(Skill skill in sk){
				skills.Add(skill);
			}
		}
		private static int next_idx = 0;
		private static Species[] s;
		static Species(){
			s = new Species[]{ // Body(0) - Equipment(1) - Warfare(2) - Support(3) - Magic(4) - Holy(5)
				new Species("Zombie",0,new List<int>{1,2,3,4,5},Skill.Slow,Skill.Shamble,Skill.Tough,Skill.Grab),
				new Species("Skeleton",4,new List<int>{0,3,4,5},Skill.Reassemble),
				new Species("Spirit",2,new List<int>{0,1,2,4,5},Skill.Fast,Skill.Possess,Skill.Fade,Skill.Incorporeal),
				new Species("WraithTODO",99,new List<int>{}),
				new Species("CinderTODO",99,new List<int>{}),
				new Species("GhoulTODO",99,new List<int>{}),
				new Species("Wight",-5,new List<int>{4,5},Skill.Absorb_Essence),
				new Species("SpectreTODO",99,new List<int>{}),
				new Species("Mummy",-5,new List<int>{1,2,5},Skill.Slow,Skill.Mummy_Rot),
				new Species("DrownedTODO",99,new List<int>{}),
				new Species("BansheeTODO",99,new List<int>{}),
				new Species("DamnedTODO",99,new List<int>{}),
				new Species("HuskTODO",99,new List<int>{}),
				new Species("Shade",0,new List<int>{5},Skill.Fragile,Skill.Fragile,Skill.Incorporeal),
				new Species("Human",0,new List<int>{}),
			};
		}
		public static Species GetIndexed(int idx){ return s[idx]; }
		public static Species Zombie{get{ return s[0]; }}
		public static Species Skeleton{get{ return s[1]; }}
		public static Species Spirit{get{ return s[2]; }}
		//public static Species Wraith{get{ return s[3]; }}
		//public static Species Cinder{get{ return s[4]; }}
		//public static Species Ghoul{get{ return s[5]; }}
		public static Species Wight{get{ return s[6]; }}
		//public static Species Spectre{get{ return s[7]; }}
		public static Species Mummy{get{ return s[8]; }}
		//public static Species Drowned{get{ return s[9]; }}
		//public static Species Banshee{get{ return s[10]; }}
		//public static Species Damned{get{ return s[11]; }}
		//public static Species Husk{get{ return s[12]; }}
		public static Species Shade{get{ return s[13]; }}
		public static Species Human{get{ return s[14]; }}
		public static Species Random{get{ return s[R.Choose(0,1,2,6,8,13)]; }}
	}
}
