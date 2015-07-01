//
using System;
using System.Collections.Generic;
using Utilities;
using PosArrays;
using Bane;
namespace Bane{
	public enum TargetingRule{AnyTile,Ally,Enemy,AllyOrEnemy,AnyActor,None};
	public class PhysicalObject{
		public pos p;
		public int row{
			get{
				return p.row;
			}
			set{
				p.row = value;
			}
		}
		public int col{
			get{
				return p.col;
			}
			set{
				p.col = value;
			}
		}
		public bool grayed_out = false;
		public bool highlighted = false;
		public bool super_todo_extra_highlighted = false;
		public bool super_todo_invisible = false;
		public PhysicalObject(){
			p = new pos(-1,-1);
		}
		public PhysicalObject(int r,int c){
			p = new pos(r,c);
		}
		public PhysicalObject(pos p){
			this.p = p;
		}
		public UI.TileCheck TileValidCheck(TargetingRule rule){ //TileValidCheck determines whether a tile is grayed out. TargetValidCheck determines whether you can actually select a given tile.
			switch(rule){
			default: //currently no exceptions
			return a=>true;
			}
		}
		public UI.ActorCheck ActorValidCheck(TargetingRule rule){
			switch(rule){
			case TargetingRule.Enemy:
			return a=>a.base_unit.team != G.active_team;
			case TargetingRule.Ally:
			return a=>a.base_unit.team == G.active_team;
			case TargetingRule.AllyOrEnemy:
			return a=>a != this;
			case TargetingRule.AnyTile:
			return a=>false;
			case TargetingRule.AnyActor:
			default:
			return a=>true;
			}
		}
		public UI.TileCheck TargetValidCheck(TargetingRule rule){
			switch(rule){
			case TargetingRule.Enemy:
			return t=>t.Ac != null && t.Ac.base_unit.team != G.active_team;
			case TargetingRule.Ally:
			return t=>t.Ac != null && t.Ac.base_unit.team == G.active_team;
			case TargetingRule.AllyOrEnemy:
			return t=>t.Ac != null && t.Ac != this.Ac;
			case TargetingRule.AnyTile:
			return a=>true;
			case TargetingRule.AnyActor:
			return t=>t.Ac != null;
			default:
			return t=>false;
			}
		}
		public Tile GetTarget(TargetingRule rule,int range){
			return UI.GetTarget(p,range,TileValidCheck(rule),ActorValidCheck(rule),TargetValidCheck(rule));
		}
		public Tile GetEnemy(int range){
			return UI.GetTarget(p,range,t=>true,a=>a.base_unit.team != G.active_team,t=>t.Ac != null && t.Ac.base_unit.team != G.active_team);
		}
		public Tile GetAlly(int range){
			return UI.GetTarget(p,range,t=>true,a=>a.base_unit.team == G.active_team,t=>t.Ac != null && t.Ac.base_unit.team == G.active_team);
		}
		public Tile GetAllyOrEnemy(int range){
			return UI.GetTarget(p,range,t=>true,a=>true,t=>t.Ac != null && t.Ac != this.Ac);
		}
		public Tile GetTile(int range){
			return UI.GetTarget(p,range,t=>true,a=>false,t=>true);
		}
		public int DistanceFrom(PhysicalObject o){ return DistanceFrom(o.row,o.col); }
		public int DistanceFrom(pos p){ return DistanceFrom(p.row,p.col); }
		public int DistanceFrom(int r,int c){
			int dy = Math.Abs(r-row);
			int dx = Math.Abs(c-col);
			return dx + dy;
		}
		public Actor actor(){
			return M.actor[p];
		}
		public Tile tile(){
			return M.tile[p];
		} // Wait a second, the 2 methods above aren't lazy enough...
		public Actor Ac{ get{ return M.actor[p]; } set{} }
		public Tile Ti{ get{ return M.tile[p]; } set{} }
		public Actor ActorInDirection(int dir){
			switch(dir){
			case 7:
			if(M.BoundsCheck(row-1,col-1)){
				return M.actor[row-1,col-1];
			}
			break;
			case 8:
			if(M.BoundsCheck(row-1,col)){
				return M.actor[row-1,col];
			}
			break;
			case 9:
			if(M.BoundsCheck(row-1,col+1)){
				return M.actor[row-1,col+1];
			}
			break;
			case 4:
			if(M.BoundsCheck(row,col-1)){
				return M.actor[row,col-1];
			}
			break;
			case 5:
			if(M.BoundsCheck(row,col)){
				return M.actor[row,col];
			}
			break;
			case 6:
			if(M.BoundsCheck(row,col+1)){
				return M.actor[row,col+1];
			}
			break;
			case 1:
			if(M.BoundsCheck(row+1,col-1)){
				return M.actor[row+1,col-1];
			}
			break;
			case 2:
			if(M.BoundsCheck(row+1,col)){
				return M.actor[row+1,col];
			}
			break;
			case 3:
			if(M.BoundsCheck(row+1,col+1)){
				return M.actor[row+1,col+1];
			}
			break;
			}
			return null;
		}
		public Tile TileInDirection(int dir){
			switch(dir){
			case 7:
			if(M.BoundsCheck(row-1,col-1)){
				return M.tile[row-1,col-1];
			}
			break;
			case 8:
			if(M.BoundsCheck(row-1,col)){
				return M.tile[row-1,col];
			}
			break;
			case 9:
			if(M.BoundsCheck(row-1,col+1)){
				return M.tile[row-1,col+1];
			}
			break;
			case 4:
			if(M.BoundsCheck(row,col-1)){
				return M.tile[row,col-1];
			}
			break;
			case 5:
			if(M.BoundsCheck(row,col)){
				return M.tile[row,col];
			}
			break;
			case 6:
			if(M.BoundsCheck(row,col+1)){
				return M.tile[row,col+1];
			}
			break;
			case 1:
			if(M.BoundsCheck(row+1,col-1)){
				return M.tile[row+1,col-1];
			}
			break;
			case 2:
			if(M.BoundsCheck(row+1,col)){
				return M.tile[row+1,col];
			}
			break;
			case 3:
			if(M.BoundsCheck(row+1,col+1)){
				return M.tile[row+1,col+1];
			}
			break;
			}
			return null;
		}
		public int DirectionOf(PhysicalObject obj){ return p.DirectionOf(obj.p); }
		public int DirectionOf(pos obj){ return p.DirectionOf(obj); }
		public List<Actor> ActorsWithinDistance(int dist){ return ActorsWithinDistance(dist,false); }
		public List<Actor> ActorsWithinDistance(int dist,bool exclude_origin){
			List<Actor> result = new List<Actor>();
			for(int i=row-dist;i<=row+dist;++i){
				for(int j=col-dist;j<=col+dist;++j){
					if(i!=row || j!=col || exclude_origin==false){
						if(M.BoundsCheck(i,j) && M.actor[i,j] != null && DistanceFrom(i,j) <= dist){
							result.Add(M.actor[i,j]);
						}
					}
				}
			}
			return result;
		}
	}
}
