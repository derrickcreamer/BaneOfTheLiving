//
using System;
using System.Collections.Generic;
using Utilities;
using PosArrays;
using Bane;
namespace Bane{
	public partial class Actor : PhysicalObject{
		private static Tile AI_target = null; //go go gadget hacks
		private static Tile AI_target2 = null;
		private static TargetResult AI_effect = null;
		private class AI_Action{
			public Tile t = null;
			public Tile t2 = null;
			public TargetResult effect = null;
			public AI_Action(Tile t_,Tile t2_,TargetResult effect_){
				t = t_;
				t2 = t2_;
				effect = effect_;
			}
			public bool Perform(){
				return effect(t,t2);
			}
		}
		public void AI_Act(){
			U.EdgeBlockedDelegate is_blocked = (p1,p2)=>{
				if(M.tile[p1].ElevationDifference(M.tile[p2]) >= 2){
					return true;
				}
				if(M.actor[p2] != null && M.actor[p2].IsEnemy(this)){
					return true;
				}
				return false;
			};
			var movemap = M.tile.GetDijkstraMap(new List<pos>{p},x=>0,is_blocked,(p1,p2)=>1);
			List<AI_Action> actions = new List<AI_Action>();
			foreach(Skill s in GetUsableActiveSkills()){
				
				//getbesttarget(s,movemap){
					//if s has a secondary target, find all tiles that match, and dijkscan from them.
					//now, find all tiles that match the primary targeting rule. If a secondary target exists, discard all tiles that are outside of 'range2' on the
					//secondary-target-map, because this primary target can't reach any of them.
					//Now that we have some primary targets (if we don't, i guess we're done) we're going to run a dijkstra scan from them.
					//iterating again, we now have a list of all tiles we could move to and target SOMETHING with this skill.
					//
				//}
				//steps per skill:
				//find best target, using movemap and dijkstras. If there are no targets (or no targets that'll *do* anything?), this skill is done, and dropped.
				//calculate priority ('fitness') of each remaining skill to see which we'll pick.
				//
				//
				//
				//if(UseSkill(s)){ //todo
				//	actions.Add(new AI_Action(AI_target,AI_target2,AI_effect));
				//}
			}

		}
		public void AI_SelectAction(){
			var active_skills = GetUsableActiveSkills();
			int sel = 0;
			if(sel == 0){
				AI_MoveAction();
				return;
			}
			if(sel == active_skills.Count + 1){
				//todo: basic attack
				return;
			}
			if(sel == active_skills.Count + 2){
				//skip turn, do nothing.
				return;
			}
			AI_UseSkill(active_skills[sel-1]);
		}
		public bool AI_MoveAction(){ return true; } //here, or in AI?
		public void AI_UseSkill(Skill s){}
	}
	/*public static class AI{ //todo: perhaps this should return an IEnum, so the AI could decide "move there, then attack", and then yield a 0 for the move, then a 3.
		//public static  t;
		public static int ChooseAction(Actor a){
			List<Skill> active_skills = a.GetUsableActiveSkills();
			int[] priority = new int[active_skills.Count + 2]; //skills, plus double-move and unarmed attack
			priority[0]--; //moving is a lower priority than any skill by default.
			int idx = 1;
			foreach(Skill s in active_skills){ //priorities at or below -10 are considered impossible and will never be chosen.
				if(s.IsAttack()){
					priority[idx]++;
				}
				priority[idx] = NewSkillPriority(a,s,priority[idx]);
				idx++;
			}
			priority[active_skills.Count+1] = NewUnarmedPriority(1,active_skills); //todo, i hate this. Just loop over them, make a bool has_attack, then subtract a bunch if it's true. No method needed.
			int highest_priority = -10;
			List<int> highest_indices = new List<int>();
			for(int i=0;i<priority.GetLength(0);++i){
				if(priority[i] > highest_priority){
					highest_priority = priority[i];
					highest_indices.Clear();
					highest_indices.Add(i);
				}
				else{
					if(priority[i] == highest_priority){
						highest_indices.Add(i);
					}
				}
			}
			if(highest_indices.Count > 0){
				return highest_indices.Random();
			}
			return active_skills.Count + 2; //this returns a selection of "Next Unit", which for AI really means "do nothing and end turn".
		}
		public static int NewSkillPriority(Actor a,Skill s,int old_priority){
			switch(s){
			case Skill.Shadow_Sneak:
			return -99;
			}
			return old_priority;
		}
		public static int NewMovePriority(Actor a,int old_priority){
			return old_priority; //todo: not sure what this is supposed to do yet. won't it also need to know the skills and current priorities?
		}
		public static int NewUnarmedPriority(int old_priority,List<Skill> skills){ //todo: add parameters as needed. CBA right now.
			return old_priority - 1;
		}
	}*/
}
