//
using System;
using PosArrays;
namespace Bane{
	public class Tile : PhysicalObject{
		public int elevation = 0;
		public static Tile Create(int r,int c,int elev){
			if(M.tile[r,c] == null){
				Tile t = new Tile();
				t.elevation = elev;
				t.p = new pos(r,c);
				M.tile[r,c] = t;
				return t;
			}
			return null;
		}
		public int ElevationDifference(Tile other){
			return Math.Abs(elevation - other.elevation);
		}
		public pos GetScreenOrigin(){
			pos dpos = M.GetDrawingPosition(this);
			int screen_y = M.MapOffset.row - elevation*G.elevation_px + (dpos.row+dpos.col)*G.tile_spacing_vertical + 40;
			int screen_x = M.MapOffset.col + (dpos.col - dpos.row + G.ROWS - 1)*G.tile_spacing_horizontal + 6;
			return new pos(screen_y,screen_x);
		}
	}
}
