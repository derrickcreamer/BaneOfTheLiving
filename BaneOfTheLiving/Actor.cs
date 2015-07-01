//
using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Graphics;
using Utilities;
using PosArrays;
using TextPanels;
using Attributes;
using Bane;
namespace Bane{
	public enum Attr{BonusHealth,BonusInitiative,BonusMovement,Stunned,Defended,MovedThisTurn,Vulnerable,Snared,Taunted,Dumbfounded,Calmed,Disarmed,Shifting,
		ManaBurned,FoggedVision};
	public class Unit{
		public int health; //2 health = 1 heart
		public int initiative;
		public int movement;
		public int team;
		public Job job;
		public Species species;
		public int human_type;
		public List<Skill> skills;
		public string name{
			get{
				if(job == Job.Commoner){
					return species.name;
				}
				return species.name + " " + job.name;
			}
		}
		private Unit(int hp,int init,int move){
			health = hp;
			initiative = init;
			movement = move;
			human_type = R.Between(0,9);
		}
		public static Unit Create(int team,Species s,Job j,Unit living){
			List<Skill> skills = new List<Skill>(s.skills);
			foreach(Skill sk in j.skills){
				if(s.blocked_skill_categories.Contains(sk.Category())){
					continue;
				}
				if(sk == Skill.Fast){
					if(skills.Contains(Skill.Slow)){
						skills.Remove(Skill.Slow);
						continue;
					}
				}
				else if(sk == Skill.Slow){
					if(skills.Contains(Skill.Fast)){
						skills.Remove(Skill.Fast);
						continue;
					}
				}
				else if(sk == Skill.Tough){
					if(skills.Contains(Skill.Fragile)){
						skills.Remove(Skill.Fragile);
						continue;
					}
				}
				else if(sk == Skill.Fragile){
					if(skills.Contains(Skill.Tough)){
						skills.Remove(Skill.Tough);
						continue;
					}
				}
				skills.Add(sk);
			}
			int health = 10;
			int init = j.initiative + s.initiative_mod;
			int movement = 3;
			foreach(Skill sk in skills){
				if(sk == Skill.Tough){
					health += 2;
				}
				else if(sk == Skill.Fragile){
					health -= 2;
				}
				else if(sk == Skill.Fast){
					movement++;
				}
				else if(sk == Skill.Slow){
					movement--;
				}
				else if(sk == Skill.Shamble){
					init = 0;
				}
			}
			Unit u = new Unit(health,init,movement);
			u.team = team;
			u.job = j;
			u.species = s;
			u.skills = skills;
			if(living != null){
				u.human_type = living.human_type;
			}
			return u;
		}
		public void Change(Species s,Job j){
			skills = new List<Skill>(s.skills);
			foreach(Skill sk in j.skills){
				if(s.blocked_skill_categories.Contains(sk.Category())){
					continue;
				}
				if(sk == Skill.Fast){
					if(skills.Contains(Skill.Slow)){
						skills.Remove(Skill.Slow);
						continue;
					}
				}
				else if(sk == Skill.Slow){
					if(skills.Contains(Skill.Fast)){
						skills.Remove(Skill.Fast);
						continue;
					}
				}
				else if(sk == Skill.Tough){
					if(skills.Contains(Skill.Fragile)){
						skills.Remove(Skill.Fragile);
						continue;
					}
				}
				else if(sk == Skill.Fragile){
					if(skills.Contains(Skill.Tough)){
						skills.Remove(Skill.Tough);
						continue;
					}
				}
				skills.Add(sk);
			}
			health = 10;;
			initiative = j.initiative + s.initiative_mod;
			movement = 3;
			foreach(Skill sk in skills){
				if(sk == Skill.Tough){
					health += 2;
				}
				else if(sk == Skill.Fragile){
					health -= 2;
				}
				else if(sk == Skill.Fast){
					movement++;
				}
				else if(sk == Skill.Slow){
					movement--;
				}
				else if(sk == Skill.Shamble){
					initiative = 0;
				}
			}
			job = j;
			species = s;
		}
		public static List<Unit> GetUnitsFromEncounterList(int[] encounter_list){
			List<Unit> result = new List<Unit>();
			Job[] jobs = new Job[]{Job.Rogue,Job.Champion,Job.Scout,Job.Healer,Job.Soldier,Job.Wizard,Job.Commoner};
			int idx = 0;
			foreach(int n in encounter_list){
				for(int i=0;i<n;++i){
					result.Add(Unit.Create(1,Species.Human,jobs[idx],null));
				}
				++idx;
			}
			return result;
		}
		public static int[] GetEncounterUnitList(int battle){
			int[] result = new int[7]; //rogue champion scout healer soldier wizard commoner
			/*if(battle == 1){
				result[6] = 2;
				add_random(result,1); todo uncomment!
				return result;
			}*/
			if(battle <= 4){
				switch(R.Roll(6)){
				case 1:
				result[6] = 6;
				add_random(result,1);
				break;
				case 2:
				result[6] = 2;
				add_random(result,2);
				break;
				case 3:
				result[6] = 2;
				add_melee(result,1);
				add_ranged(result,1);
				break;
				case 4:
				result[6] = 2;
				add_fighter(result,1);
				result[3] = 1;
				break;
				case 5:
				result[6] = 2;
				add_artillery(result,1);
				result[1] = 1;
				break;
				case 6:
				add_fighter(result,1);
				add_artillery(result,1);
				result[3] = 1;
				break;
				}
				return result;
			}
			if(battle <= 8){
				switch(R.Roll(6)){
				case 1:
				result[6] = 8;
				add_random(result,2);
				break;
				case 2:
				result[6] = 4;
				add_random(result,3);
				break;
				case 3:
				result[6] = 2;
				add_random(result,4);
				break;
				case 4:
				add_random(result,6);
				break;
				case 5:
				add_melee(result,3);
				add_ranged(result,2);
				break;
				case 6:
				add_fighter(result,1);
				result[0] = 1;
				result[3] = 1;
				result[5] = 1;
				break;
				}
				return result;
			}
			switch(R.Roll(7)){
			case 1:
			add_random(result,8);
			break;
			case 2:
			result[6] = 8;
			add_random(result,4);
			break;
			case 3:
			result[R.Between(0,5)] = 8;
			break;
			case 4:
			add_melee(result,4);
			add_artillery(result,3);
			result[3] = 1;
			break;
			case 5:
			result[4] = 5;
			result[3] = 3;
			break;
			case 6:
			add_artillery(result,8);
			break;
			case 7:
			add_melee(result,10);
			break;
			}
			if(battle <= 16){
				return result;
			}
			else{
				add_random(result,battle/5 - 2);
				return result;
			}
		}
		private static void add_random(int[] a,int count){
			for(int i=0;i<count;++i){
				a[R.Between(0,5)]++;
			}
		}
		private static void add_melee(int[] a,int count){
			for(int i=0;i<count;++i){
				a[R.Choose(0,1,4)]++;
			}
		}
		private static void add_ranged(int[] a,int count){
			for(int i=0;i<count;++i){
				a[R.Choose(2,3,5)]++;
			}
		}
		private static void add_fighter(int[] a,int count){
			for(int i=0;i<count;++i){
				a[R.Choose(1,4)]++;
			}
		}
		private static void add_artillery(int[] a,int count){
			for(int i=0;i<count;++i){
				a[R.Choose(2,5)]++;
			}
		}
	}
	public partial class Actor : PhysicalObject{
		public Unit base_unit;
		public int facing; //facing is aligned with the spritesheet: 0-3, starting at southwest facing and going clockwise.
		public int damage;
		public AttributeDict<Attr> attr = new AttributeDict<Attr>();
		public List<Attribute<Attr>> start_of_turn_expiration = new List<Attribute<Attr>>();
		public List<Attribute<Attr>> end_of_turn_expiration = new List<Attribute<Attr>>(); //todo! Attrs created THIS action shouldn't expire at the end of THIS action.
		public int health{ get{ return max_health - damage; } }
		public int max_health{ get{ return base_unit.health + attr[Attr.BonusHealth]; } }
		public int initiative{ get{ return base_unit.initiative + attr[Attr.BonusInitiative]; } }
		public int movement{ get{ return base_unit.movement + attr[Attr.BonusMovement]; } }
		public int team{ get{ return base_unit.team; } }
		private Actor(Unit u){
			base_unit = u;
			facing = R.Between(0,3);
		}
		static Actor(){
			AttributeDict<Attr>.get_time = () => Q.now;
			AttributeDict<Attr>.expires_at_exact_time = false; //todo check this value too.
		}
		public static Actor Create(Unit u){
			return new Actor(u);
		}
		public bool Move(PhysicalObject o){ return Move(o.row,o.col); }
		public bool Move(pos p){ return Move(p.row,p.col); }
		public bool Move(int r,int c){
			if(M.actor[r,c] == null){
				TurnToFace(M.tile[r,c]);
				M.actor[r,c] = this;
				M.actor[row,col] = null;
				p = new pos(r,c);
			}
			else{
				return false; //todo swap
			}
			//make sure Move swaps places, OR, require an explicit Swap()
			return true; //todo
		}
		public bool HasAttr(Attr a){ return attr.Has(a); }
		public void Add(Attribute<Attr> a){
			attr += a;
		}
		public static Actor operator +(Actor ac,Attribute<Attr> a){
			ac.attr += a;
			return ac;
		}
		public Attribute<Attr> UntilStartOfAction(Attribute<Attr> a){
			start_of_turn_expiration.Add(a);
			return a;
		}
		public Attribute<Attr> UntilEndOfAction(Attribute<Attr> a){
			end_of_turn_expiration.Add(a);
			return a;
		}
		public int RotatedFacing(){ //todo: move this? rename this? It doesn't seem to fit here.
			return (facing + M.Rotation).Modulo(4);
		}
		public void TurnToFace(PhysicalObject o){
			int dir = DirectionOf(o);
			switch(dir){
			case 6:
			facing = 0;
			break;
			case 2:
			facing = 1;
			break;
			case 4:
			facing = 2;
			break;
			case 8:
			facing = 3;
			break;
			}
		}
		public int FacingDirection(){
			switch(facing){
			case 0:
			return 6;
			case 1:
			return 2;
			case 2:
			return 4;
			case 3:
			default:
			return 8;
			}
		}
		public bool IsFacingAwayFrom(PhysicalObject o){
			return o.DirectionOf(this) == FacingDirection();
		}
		public bool IsEnemy(Actor other){
			return team != other.team;
		}
		public pos GetScreenOrigin(){
			pos tp = Ti.GetScreenOrigin();
			return new pos(tp.row - 44,tp.col + 30);
		}
		public bool MoveAction(){
			//a lot like targeting.
			//keep a list of tiles (the origin isn't included, I think)
			//moving the cursor is subject to the same rules as targeting (and might also mark illegal movements the same way).
			//moving the cursor to an unlisted tile adds that tile to the list.
			//moving the cursor to a listed tile cuts off all the tiles after it in the list.
			//  -this makes it hard to move to the same tile more than once in a single turn. perhaps a modifier like Control could allow this?
			//escape and enter work as expected.
			List<Tile> path = new List<Tile>();
			Tile current = Ti;
			int range = movement;
			foreach(Tile t in M.tile){ //todo: check for movement restrictions etc.
				if(DistanceFrom(t) > range){
					t.grayed_out = true;
					if(t.Ac != null){
						t.Ac.grayed_out = true;
					}
				}
				else{
					bool illegal_move = false;
					if(illegal_move){
						t.grayed_out = true;
					}
					//if(t.Ac != null && !actor_valid_condition(t.Ac)){ //todo: moving through allies?
					//	t.Ac.grayed_out = true;
					//}
				}
			}
			current.highlighted = true;
			current.super_todo_extra_highlighted = true;
			/*if(current.Ac != null){
				current.Ac.highlighted = true;
			}*/
			G.terrain.Update();
			G.living.Update();
			G.undead.Update();
			bool result = false;
			bool done = false;
			while(!done){
				Command command = G.GetCommand();
				switch(command){
				case Command.Click:
				if(current.DistanceFrom(G.LastTile) == 1){
					int dir = G.ScreenDirectionFromTrueDirection(current.DirectionOf(G.LastTile));
					switch(dir){
					case 8:
					command = Command.Up;
					break;
					case 2:
					command = Command.Down;
					break;
					case 4:
					command = Command.Left;
					break;
					case 6:
					command = Command.Right;
					break;
					}
					goto case Command.Right;
				}
				break;
				case Command.Up:
				case Command.Down:
				case Command.Left:
				case Command.Right:
				{
					int dir = G.RotatedDirectionFromInput(command);
					if(current.TileInDirection(dir) != null){
						/*if(current.TileInDirection(dir).DistanceFrom(this) > range){
							Tile cwt = current.TileInDirection(dir.RotateEightWayDir(true));
							Tile ccwt = current.TileInDirection(dir.RotateEightWayDir(false));
							bool cw = cwt != null && cwt.DistanceFrom(this) <= range;
							bool ccw = ccwt != null && ccwt.DistanceFrom(this) <= range;
							if(cw && !ccw){
								dir = dir.RotateEightWayDir(true);
							}
							if(!cw && ccw){
								dir = dir.RotateEightWayDir(false); //todo: you can re-enable this code if you make it remove the proper tile.
							}
						}*/
						if(current.TileInDirection(dir).DistanceFrom(this) <= range && current.ActorInDirection(dir) == null){ //todo: allow ally passage?
							current.super_todo_extra_highlighted = false;
							/*if(M.actor[current.p] != null){
								M.actor[current.p].highlighted = false;
							}*/
							current = current.TileInDirection(dir);
							current.super_todo_extra_highlighted = true;
							/*if(M.actor[current.p] != null){
								M.actor[current.p].highlighted = true;
							}*/
							if(path.Contains(current)){
								if(current != path.Last()){
									foreach(Tile t in path){
										t.highlighted = false;
									}
									int idx = path.LastIndexOf(current);
									path.RemoveRange(idx+1,path.Count - (idx+1));
								}
							}
							else{
								path.Add(current); //todo: rearrange this so it checks distance FIRST.
							}
							foreach(Tile t in path){
								t.highlighted = true;
							}
							G.terrain.Update();
							G.living.Update();
							G.undead.Update();
						}
					}
					break;
				}
				case Command.Enter:
				foreach(Tile t in path){
					G.living.Update();
					G.undead.Update();
					G.Window.DrawSurfaces();
					System.Threading.Thread.Sleep(100);
					Move(t);
				}
				result = true;
				done = true;
				break;
				case Command.Escape:
				result = false;
				done = true;
				break;
				default:
				break;
				}
			}
			foreach(Tile t in M.tile){
				t.grayed_out = false;
				t.highlighted = false;
				t.super_todo_extra_highlighted = false;
				if(t.Ac != null){
					t.Ac.grayed_out = false;
					t.Ac.highlighted = false;
					t.Ac.super_todo_extra_highlighted = false;
				}
			}
			G.terrain.Update();
			G.living.Update();
			G.undead.Update();
			return result;
		}
		public List<Skill> GetUsableActiveSkills(){
			List<Skill> result = new List<Skill>();
			result.Add(Skill.Move);
			foreach(Skill s in base_unit.skills){
				if(s.Active()){ //todo: and not disabled...
					result.Add(s);
				}
			}
			result.Add(Skill.UnarmedAttack);
			return result;
		}
		public void Act(){
			if(team != 0){
				AI_Act();
				return;
			}
			List<Skill> actions = GetUsableActiveSkills();
			List<string> names = new List<string>();
			foreach(Skill s in actions){
				names.Add(s.Name());
			}
			names.Add("Next Unit");
			int sel = UI.CursorSelection(names,base_unit.name,this);
			if(sel == -1){ //todo: should Escape here be the same as Next Unit? Bring up a menu? Or what?
				return;
			}
			if(sel == actions.Count){
				UI.TODORENAME();
				Q.current_event.actors.Add(this); //Moves to the next unit. Adds another reference to the end of the list so this unit still gets a turn later.
				return;
			}
			UseSkill(actions[sel]);
		}
		public void Heal(int value){
			damage -= value.Cap(damage);
		}
		public bool TakeDamage(int dmg,Actor source){ //returns true if it survived
			damage += dmg;
			if(damage >= health){
				M.actor[p] = null; //todo kill properly
				return false;
			}
			return true;
		}
		private static Attr[] status_list = new Attr[]{Attr.Snared,Attr.Stunned,Attr.Taunted,Attr.Vulnerable}; //todo!
		public List<Attr> GetStatuses(){
			List<Attr> result = new List<Attr>();
			foreach(Attr status in status_list){
				if(HasAttr(status)){
					result.Add(status);
				}
			}
			return result;
		}
		private delegate Tile TargetDelegate(int range);
		private delegate TargetDelegate SecondaryTargetDelegate(Tile source);
		public delegate bool TargetResult(Tile t,Tile t2);
		/*private bool apply_skill(TargetingRule rule,int range,TargetingRule rule2,int range2,TargetResult effect){
			return apply_skill(rule,range,TargetingRule.None,0,effect);
		}
		private bool apply_skill(TargetingRule rule,int range,TargetingRule rule2,int range2,TargetResult effect){
			Tile t = GetTarget(rule,range);
			if(t == null){
				return false;
			}
			Tile t2 = null;
			if(rule != TargetingRule.None){
				t2 = t.GetTarget(rule,range);
				if(t2 == null){
					return false;
				}
			}
			return effect(t,t2);
		}*/
		private bool apply_skill(TargetDelegate get_t,int range,TargetResult effect){
			return apply_skill(get_t,range,null,0,effect);
		}
		private bool apply_skill(TargetDelegate get_t,int range,SecondaryTargetDelegate get_t2,int range2,TargetResult effect){
			Tile t = get_t(range);
			if(t == null){
				return false;
			}
			Tile t2 = null;
			if(get_t2 != null){
				TargetDelegate g2 = get_t2(t);
				t2 = g2(range2);
				if(t2 == null){
					return false;
				}
			}
			if(team != 0){
				AI_target = t;
				AI_target2 = t2;
				AI_effect = effect;
				return true;
			}
			return effect(t,t2);
		}
		/*public bool UseSkill(Skill s){
			if(team == 0){
				return apply_skill(s.FirstTarget(),s.FirstRange(),s.SecondTarget(),s.SecondRange(),s.Effect());
			}
			else{
				return true; //todo, do AI stuff.
			}
		}*/
		public bool UseSkill(Skill skill){
			switch(skill){
			case Skill.Move:
			if(team == 0){
				return MoveAction();
			}
			else{
				return AI_MoveAction();
			}
			case Skill.UnarmedAttack:
			return apply_skill(GetEnemy,1,(t,t2)=>{
				t.Ac.TakeDamage(2,this);
				return true;
			});
			case Skill.Alarm:
			return apply_skill(GetAlly,6,t=>t.GetTile,1,(t,t2)=>{
				t.Ac.TurnToFace(t2);
				t.Ac += Attr.BonusInitiative.Ready(2,this,Q.next.start);
				return true;
			});
			case Skill.Arcane_Babble:
			return apply_skill(GetAlly,3,(t,t2)=>{
				var list = GetStatuses();
				List<string> s = new List<string>();
				foreach(var a in list){
					s.Add(a.Name());
				}
				if(s.Count == 0){
					return false;
				}
				int sel = UI.CursorSelection(s,"Choose a status:");
				if(sel != -1){
					t.Ac.attr[list[sel]] = 0; //todo: make sure this cancels properly
					t.Ac += t.Ac.UntilEndOfAction(Attr.Dumbfounded.Ready(this));
					return true;
				}
				return false;
			});
			case Skill.Bow:
			return apply_skill(GetEnemy,6,(t,t2)=>{
				int dmg = 2;
				if(HasAttr(Attr.MovedThisTurn)){
					dmg = 1;
				}
				t.Ac.TakeDamage(dmg,this);
				return true;
			});
			case Skill.Bring_Low:
			return apply_skill(GetEnemy,1,(t,t2)=>{
				if(t.Ac.health > health){
					t.Ac.TakeDamage(t.Ac.health - health,this); //todo: damage or HP loss?
				}
				return true;
			});
			case Skill.Calming_Voice:
			return apply_skill(GetAlly,3,(t,t2)=>{
				var list = GetStatuses();
				List<string> s = new List<string>();
				foreach(var a in list){
					s.Add(a.Name());
				}
				if(s.Count == 0){
					return false;
				}
				int sel = UI.CursorSelection(s,"Choose a status:");
				if(sel != -1){
					t.Ac.attr[list[sel]] = 0; //todo: make sure this cancels properly
					t.Ac += t.Ac.UntilEndOfAction(Attr.Calmed.Ready(this));
					return true;
				}
				return false;
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
			case Skill.Chill:
			return apply_skill(GetEnemy,6,(t,t2)=>{
				if(t.Ac.TakeDamage(1,this)){
					t.Ac += Attr.BonusInitiative.Ready(-1,this,Q.next.start);
				}
				return true;
			});
			case Skill.Dagger:
			return apply_skill(GetEnemy,1,(t,t2)=>{
				if(t.Ac.TakeDamage(2,this) && t.Ac.IsFacingAwayFrom(this)){
					t.Ac += Attr.Stunned.Ready(this,Q.now.end);
				}
				return true;
			});
			case Skill.Defend_Ally:
			return apply_skill(GetAlly,1,(t,t2)=>{
				t.Ac += this.UntilStartOfAction(Attr.Defended.Ready(this));
				return true;
			});
			case Skill.Distract:
			return apply_skill(GetEnemy,1,(t,t2)=>{
				t.Ac.TurnToFace(this);
				t.Ac += Attr.Vulnerable.Ready(this,Q.now.end);
				return true;
			});
			case Skill.Dodge:
			return false; //todo remove, remake, or reconsider
			case Skill.Fog_Vision:
			return apply_skill(GetEnemy,6,(t,t2)=>{
				t.Ac += t.Ac.UntilEndOfAction(Attr.FoggedVision.Ready(this));
				return true;
			});
			case Skill.Hammer:
			return apply_skill(GetEnemy,1,(t,t2)=>{
				if(t.Ac.TakeDamage(2,this)){
					t.Ac += Attr.BonusInitiative.Ready(-1,this,Q.next.start);
				}
				return true;
			});
			case Skill.Hold:
			return apply_skill(GetEnemy,3,(t,t2)=>{
				t.Ac += t.Ac.UntilEndOfAction(Attr.Snared.Ready(this));
				return true;
			});
			case Skill.Hurry:
			return apply_skill(GetAlly,6,(t,t2)=>{
				Event bracket = null;
				Event ally_bracket = null;
				foreach(Event e in Q.list){
					if(e.type == EventType.Action){
						if(e.actors.Contains(this)){
							bracket = e;
						}
						if(e.actors.Contains(t.Ac)){
							ally_bracket = e;
						}
					}
				}
				if(bracket != null && ally_bracket != null && bracket.time.initiative_count > ally_bracket.time.initiative_count){
					ally_bracket.actors.Remove(t.Ac);
					bracket.actors.Add(t.Ac);
					return true;
				}
				return false;
			});
			case Skill.Invigorate:
			return apply_skill(GetAlly,1,(t,t2)=>{
				if(t.Ac.health < health){
					t.Ac.Heal(health - t.Ac.health);
					return true;
				}
				return false;
			});
			case Skill.Lay_Down_Arms:
			foreach(Actor a in ActorsWithinDistance(4)){
				Actor ac = a;
				ac += UntilStartOfAction(Attr.Disarmed.Ready(this));
			}
			return true;
			case Skill.Longsword:
			return apply_skill(GetEnemy,1,(t,t2)=>{
				int dmg = 2;
				if(t.Ac.health <= 2){
					dmg += 10;
				}
				t.Ac.TakeDamage(dmg,this);
				return true;
			});
			case Skill.Mana_Burn:
			return apply_skill(GetEnemy,4,(t,t2)=>{
				t.Ac += Attr.ManaBurned.Ready(this); //todo: does this last forever?
				return true;
			});
			case Skill.Mark_Target:
			return apply_skill(GetEnemy,6,(t,t2)=>{
				t.Ac += Attr.Vulnerable.Ready(this,Q.now.end);
				return true;
			});
			case Skill.Maul:
			return apply_skill(GetEnemy,1,(t,t2)=>{
				int dmg = 2;
				if(t.Ac.health == t.Ac.max_health){
					dmg++;
				}
				t.Ac.TakeDamage(dmg,this);
				return true;
			});
			case Skill.Mend:
			return apply_skill(GetAlly,5,(t,t2)=>{
				t.Ac.Heal(1);
				return true;
			});
			case Skill.Possess:
			return apply_skill(GetEnemy,1,(t,t2)=>{
				List<string> actions = new List<string>();
				List<Skill> active_skills = new List<Skill>();
				foreach(Skill s in t.Ac.base_unit.skills){
					if(s.Active()){
						actions.Add(s.Name());
						active_skills.Add(s);
					}
				}
				if(actions.Count == 0){
					return false;
				}
				int sel = UI.CursorSelection(actions,t.Ac.base_unit.name);
				if(sel != -1){
					return t.Ac.UseSkill(active_skills[sel]);
				}
				return false;
			});
			case Skill.Push:
			return apply_skill(GetEnemy,1,(t,t2)=>{
				Tile dest = t.TileInDirection(this.DirectionOf(t));
				if(dest != null){
					t.Ac.Move(dest); //todo checks etc. Also does this push farther if they're vulnerable?
					return true;
				}
				return false;
			});
			case Skill.Reposition:
			return apply_skill(GetAlly,2,t=>t.GetTile,1,(t,t2)=>{
				t.Ac.attr[Attr.Shifting,this] = 1;
				t.Ac.Move(t2);
				t.Ac.attr[Attr.Shifting,this] = 0;
				t.Ac.attr[Attr.Vulnerable] = 0;
				return true;
			});
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
			case Skill.Swap:
			return apply_skill(GetAlly,3,(t,t2)=>{
				Move(t);
				return true;
			});
			case Skill.Taunt:
			return apply_skill(GetEnemy,1,(t,t2)=>{
				t.Ac.TurnToFace(this);
				t.Ac += t.Ac.UntilEndOfAction(Attr.Taunted.Ready(this));
				return true;
			});
			case Skill.Warning:
			return apply_skill(GetAlly,6,t=>t.GetTile,1,(t,t2)=>{
				t.Ac.TurnToFace(t2);
				t.Ac += this.UntilEndOfAction(Attr.Defended.Ready(this));
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
			}
			return false;
		}
	}
}
