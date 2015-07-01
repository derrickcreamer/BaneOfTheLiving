//
using System;
using System.Collections.Generic;
using System.Drawing;
using PosArrays;
using Utilities;
namespace Bane{
	public static class M{
		public static PosArray<Tile> tile;
		public static PosArray<Actor> actor;
		public static int Rotation = 0;
		public static int cutoff_elevation;
		public static List<IMouseTarget> dragged_objects = new List<IMouseTarget>(); // When the map is dragged, anything in this list will move with it.
		private static pos map_offset = new pos(0,0);
		public static pos MapOffset{
			get{
				return map_offset;
			}
			set{
				map_offset = value;
				G.terrain.SetOffsetInPixels(map_offset.col + 7,map_offset.row + 40);
				G.undead.SetOffsetInPixels(map_offset.col + 11,map_offset.row - 21);
				G.living.SetOffsetInPixels(map_offset.col + 11,map_offset.row - 21);
			}
		}
		public static void SetMapOffset(int x,int y){
			int dx = x - map_offset.col;
			int dy = y - map_offset.row;
			map_offset = new pos(y,x);
			G.terrain.SetOffsetInPixels(map_offset.col + 7,map_offset.row + 40);
			G.undead.SetOffsetInPixels(map_offset.col + 11,map_offset.row - 21);
			G.living.SetOffsetInPixels(map_offset.col + 11,map_offset.row - 21);
			foreach(IMouseTarget t in dragged_objects){
				t.Move(dx,dy);
			}
		}
		public static void ChangeMapOffset(int dx,int dy){
			map_offset = new pos(map_offset.row + dy,map_offset.col + dx);
			G.terrain.SetOffsetInPixels(map_offset.col + 7,map_offset.row + 40);
			G.undead.SetOffsetInPixels(map_offset.col + 11,map_offset.row - 21);
			G.living.SetOffsetInPixels(map_offset.col + 11,map_offset.row - 21);
			foreach(IMouseTarget t in dragged_objects){
				t.Move(dx,dy);
			}
		}
		private static int ROWS{ get{ return G.ROWS; } }
		private static int COLS{ get{ return G.COLS; } }

		/*static M(){
			drag_update = ChangeMapOffset;
		}*/
		public static DragTracker GetMapDrag(Point p){
			DragTracker d = new DragTracker();
			d.target = null;
			d.update = SetMapOffset;
			d.start_location = p;
			d.click_offset_from_target_origin = new Point(p.X - map_offset.col,p.Y - map_offset.row);
			return d;
		}
		public static bool BoundsCheck(pos p,bool disallow_map_edges = false){ return BoundsCheck(p.row,p.col,disallow_map_edges); }
		public static bool BoundsCheck(int r,int c,bool disallow_map_edges = false){
			if(disallow_map_edges){
				if(r >= 1 && r < ROWS-1 && c >= 1 && c < COLS-1){
					return true;
				}
			}
			else{
				if(r >= 0 && r < ROWS && c >= 0 && c < COLS){
					return true;
				}
			}
			return false;
		}
		public static Tile GetTile(pos p){ return GetTile(p.row,p.col); }
		public static Tile GetTile(int r,int c){
			if(BoundsCheck(r,c)){
				return tile[r,c];
			}
			return null;
		}
		public static Actor GetActor(pos p){ return GetActor(p.row,p.col); }
		public static Actor GetActor(int r,int c){
			if(BoundsCheck(r,c)){
				return actor[r,c];
			}
			return null;
		}
		public static Tile TileFromDrawingPosition(pos drawing_position){
			return TileFromDrawingPosition(drawing_position.row,drawing_position.col);
		}
		public static Tile TileFromDrawingPosition(int drawing_row,int drawing_col){
			switch(M.Rotation){
			case 0:
			return GetTile(drawing_row,drawing_col);
			case 3:
			return GetTile(drawing_col,COLS-1 - drawing_row);
			case 2:
			return GetTile(ROWS-1 - drawing_row,COLS-1 - drawing_col);
			case 1:
			default:
			return GetTile(ROWS-1 - drawing_col,drawing_row);
			}
		}
		public static Actor ActorFromDrawingPosition(pos drawing_position){
			return ActorFromDrawingPosition(drawing_position.row,drawing_position.col);
		}
		public static Actor ActorFromDrawingPosition(int drawing_row,int drawing_col){
			switch(M.Rotation){
			case 0:
			return GetActor(drawing_row,drawing_col);
			case 3:
			return GetActor(drawing_col,COLS-1 - drawing_row);
			case 2:
			return GetActor(ROWS-1 - drawing_row,COLS-1 - drawing_col);
			case 1:
			default:
			return GetActor(ROWS-1 - drawing_col,drawing_row);
			}
		}
		public static pos GetDrawingPosition(PhysicalObject o){
			switch(M.Rotation){
			case 0:
			return o.p;
			case 1:
			return new pos(o.col,COLS-1 - o.row);
			case 2:
			return new pos(ROWS-1 - o.row,COLS-1 - o.col);
			case 3:
			default:
			return new pos(ROWS-1 - o.col,o.row);
			}
		}
		public static void MoveToShow(Actor a){
			const int actor_h = 81;
			const int actor_w = 49;
			pos p = a.GetScreenOrigin();
			int dx = 0;
			int dy = 0;
			if(p.col < 0){
				dx = - p.col;
			}
			if(p.col + actor_w > G.Window.ClientRectangle.Width - 1){
				dx = G.Window.ClientRectangle.Width - 1 - p.col - actor_w;
			}
			if(p.row < 0){
				dy = -p.row;
			}
			if(p.row + actor_h > G.Window.ClientRectangle.Height - 1){
				dy = G.Window.ClientRectangle.Height - 1 - p.row - actor_h;
			}
			if(dx != 0 || dy != 0){
				ChangeMapOffset(dx,dy);
			}
		}
		public static void CreateMap(){
			tile = new PosArray<Tile>(ROWS,COLS);
			actor = new PosArray<Actor>(ROWS,COLS);
			var noise = U.GetNoise(ROWS,COLS);
			//float min = 999.0f;
			//float max = -999.0f;
			cutoff_elevation = -5;
			for(int i=0;i<ROWS;++i){
				for(int j=0;j<COLS;++j){
					/*if(noise[i,j] > max){
						max = noise[i,j];
					}
					if(noise[i,j] < min){
						min = noise[i,j];
					}*/
					//Tile.Create(i,j,0);
					Tile.Create(i,j,(int)(noise[i,j] * 6.0f)); //was 9.0 (but that was too much)
					if(tile[i,j].elevation - 4 < cutoff_elevation){
						cutoff_elevation = tile[i,j].elevation - 5;
					}
					//Tile.Create(i,j,R.Between(-2,2));
				}
			}
		}
		public static void DebugCreateRandomActors(){
			actor = new PosArray<Actor>(ROWS,COLS);
			for(int n=0;n<50;++n){
				pos p = actor.RandomPosition(true);
				actor[p] = Actor.Create(Unit.Create(R.Between(2,4),Species.Random,Job.Random,null));
				actor[p].p = p;
				//Actor a = actor[p];
				/*Console.Error.WriteLine(a.species.name + " " + a.job.name);
				Console.Error.WriteLine("Health: {0,2}   Initiative: {1,2}   Movement: {2} ",a.health,a.initiative,a.movement);
				foreach(Skill sk in a.skills){
					Console.Error.WriteLine("{0,-20}",sk.name);
				}
				Console.Error.WriteLine();
				Console.Error.WriteLine();*/
			}
		}
	}
}
