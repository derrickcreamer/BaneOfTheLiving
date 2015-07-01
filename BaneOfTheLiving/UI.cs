//
using System;
using System.Collections.Generic;
using OpenTK.Graphics;
using GLDrawing;
using PosArrays;
using Utilities;
using TextPanels;
namespace Bane{
	public static class UI{
		/*public static TextPanel CreatePanel(int rows,int cols,int vert_offset_px,int horiz_offset_px){
			return TextPanel.Create(G.Window,rows,cols,16,8,vert_offset_px,horiz_offset_px,"font8x16.bmp",8,1);
		}
		public static TextPanel CreatePanel(int rows,int cols,int vert_offset_px,int horiz_offset_px,int corner){
			switch(corner){
			case 0:
			return TextPanel.Create(G.Window,rows,cols,16,8,vert_offset_px,horiz_offset_px,"font8x16.bmp",8,1);
			case 1:
			return TextPanel.Create(G.Window,rows,cols,16,8,vert_offset_px,G.Window.ClientRectangle.Width - cols*8 - horiz_offset_px,"font8x16.bmp",8,1);
			case 2:
			return TextPanel.Create(G.Window,rows,cols,16,8,G.Window.ClientRectangle.Height - rows*16 - vert_offset_px,G.Window.ClientRectangle.Width - cols*8 - horiz_offset_px,"font8x16.bmp",8,1);
			case 3:
			default:
			return TextPanel.Create(G.Window,rows,cols,16,8,G.Window.ClientRectangle.Height - rows*16 - vert_offset_px,horiz_offset_px,"font8x16.bmp",8,1);
			}
		}*/
		public delegate bool TileCheck(Tile t);
		public delegate bool ActorCheck(Actor a);
		public static Tile GetTarget(pos source,int range,TileCheck tile_valid_condition,ActorCheck actor_valid_condition,TileCheck target_valid_condition){
			Tile current = M.tile[source];
			foreach(Tile t in M.tile){
				if(t.p.DistanceFrom(source) > range){
					t.grayed_out = true;
					if(M.actor[t.p] != null){
						M.actor[t.p].grayed_out = true;
					}
				}
				else{
					if(!tile_valid_condition(t)){
						t.grayed_out = true;
					}
					if(t.Ac != null && !actor_valid_condition(t.Ac)){
						t.Ac.grayed_out = true;
					}
				}
			}
			current.highlighted = true;
			if(current.Ac != null){ //todo: not sure if this should be here. maybe just the terrain should be highlighted.
				current.Ac.highlighted = true;
			}
			G.terrain.Update();
			G.living.Update();
			G.undead.Update();
			bool done = false;
			while(!done){
				Command command = G.GetCommand();
				switch(command){
				case Command.Up:
				case Command.Down:
				case Command.Left:
				case Command.Right:
				{
					int dir = G.RotatedDirectionFromInput(command);
					if(current.TileInDirection(dir) != null){
						if(current.TileInDirection(dir).DistanceFrom(source) > range){
							Tile cwt = current.TileInDirection(dir.RotateEightWayDir(true));
							Tile ccwt = current.TileInDirection(dir.RotateEightWayDir(false));
							bool cw = cwt != null && cwt.DistanceFrom(source) <= range;
							bool ccw = ccwt != null && ccwt.DistanceFrom(source) <= range;
							if(cw && !ccw){
								dir = dir.RotateEightWayDir(true);
							}
							if(!cw && ccw){
								dir = dir.RotateEightWayDir(false);
							}
						}
						if(current.TileInDirection(dir).DistanceFrom(source) <= range){
							current.highlighted = false;
							if(M.actor[current.p] != null){
								M.actor[current.p].highlighted = false;
							}
							current = current.TileInDirection(dir);
							current.highlighted = true;
							if(M.actor[current.p] != null){
								M.actor[current.p].highlighted = true;
							}
							G.terrain.Update();
							G.living.Update();
							G.undead.Update();
						}
					}
					break;
				}
				case Command.Enter:
				if(target_valid_condition(current)){
					done = true;
				}
				break;
				case Command.Escape:
				current = null;
				done = true;
				break;
				default:
				break;
				}
			}
			foreach(Tile t in M.tile){
				t.grayed_out = false;
				t.highlighted = false;
				if(M.actor[t.p] != null){
					M.actor[t.p].highlighted = false;
					M.actor[t.p].grayed_out = false;
				}
			}
			G.terrain.Update();
			G.living.Update();
			G.undead.Update();
			return current;
		}
		public static int CursorSelection(IList<string> list,string box_title,Actor a = null){
			int box_w = Math.Max(20,box_title.Length + 2);
			ButtonPanel t = ButtonPanel.Create(list.Count + 2,box_w,0,0,2);
			if(a != null){ //if an actor was supplied, move the panel to a good location near it.
				pos p = a.GetScreenOrigin();
				pos new_offset = new pos(0,0);
				const int actor_h = 81;
				const int actor_w = 49; //todo! move these to global or something - they're also used in M.MoveToShow()
				if(p.row + actor_h + 5 + t.HeightPx < G.Window.ClientRectangle.Height){ //5 is arbitrary, just for spacing
					new_offset.row = p.row + actor_h + 5;
					new_offset.col = p.col - t.WidthPx/2 + actor_w/2; //box appears below unit
				}
				else{
					if(p.col > t.WidthPx){
						new_offset.row = p.row - t.HeightPx/2 + actor_h/2; //box appears to the left
						new_offset.col = p.col - t.WidthPx;
					}
					else{
						new_offset.row = p.row - t.HeightPx/2 + actor_h/2; //box appears to the right - should never need to appear above the unit.
						new_offset.col = p.col + actor_w;
					}
				}
				if(new_offset.row < 0){
					new_offset.row = 0;
				}
				if(new_offset.row + t.HeightPx > G.Window.ClientRectangle.Height){
					new_offset.row = G.Window.ClientRectangle.Height - t.HeightPx;
				}
				if(new_offset.col < 0){
					new_offset.col = 0;
				}
				if(new_offset.col + t.WidthPx > G.Window.ClientRectangle.Width){
					new_offset.col = G.Window.ClientRectangle.Width - t.WidthPx;
				}
				t.MoveTo(new_offset.col,new_offset.row);
				M.dragged_objects.Add(t);
			}
			//t.MainButton = t.GetFittedButton(1,1,list.Count,box_w-2,Command.None);
			t.MainButton.draggable = true;
			//t.MainButton.drag_update += t.Move;
			Color4 border_color = Color4.White.ChangeAlpha(0.7f);
			//TextPanel t = UI.CreatePanel(list.Count + 2,20,0,0,2);
			/*var sprite = UI.CreateSingleSprite(Unit.Create(0,Species.Human,Job.Random,null),0,0);
			var button = new ButtonImage(Command.None,-1,0,0,80,50,sprite.ToArray());
			button.MoveTo(t.MainButton.rect.X,t.MainButton.rect.Y);
			t.MainButton.AddButton(button);
			button.draggable = true;
			button.locked_vertical = true; //todo of course*/
			//MouseUI.AddButton(button);
			int sel = 0;
			while(true){
				//t.Clear(Color4.LightSeaGreen);
				t.Clear(border_color);
				const int char_offset = 23;
				t.Write(0,0,(box_title + " ").PadOuter(box_w),Color4.Black,border_color);
				t.Write(0,0,(char)char_offset,border_color,Color4.Transparent);
				t.Write(0,t.Cols-1,(char)(char_offset+1),border_color,Color4.Transparent);
				t.Write(t.Rows-1,t.Cols-1,(char)(char_offset+2),border_color,Color4.Transparent);
				t.Write(t.Rows-1,0,(char)(char_offset+3),border_color,Color4.Transparent);
				int idx = 1;
				foreach(string s in list){
					Color4 color = Color4.Teal.ChangeAlpha(0.7f);
					if(idx-1 == sel){
						color = Color4.DodgerBlue;
					}
					t.ButtonWrite(idx,1,(s + " ").PadOuter(box_w-2),Color4.White,color,Command.Selection,idx-1);
					++idx;
				}
				Command command = G.GetCommand();
				switch(command){
				case Command.Up:
				sel = (sel-1).Modulo(list.Count);
				break;
				case Command.Down:
				sel = (sel+1).Modulo(list.Count);
				break;
				case Command.Selection:
				sel = G.LastSelection;
				goto case Command.Enter;
				/*if(sel == G.LastSelection){
				}
				else{
				}
				break;*/
				case Command.Escape:
				t.Remove();
				M.dragged_objects.Remove(t);
				return -1;
				case Command.Enter:
				t.Remove();
				M.dragged_objects.Remove(t);
				return sel;
				default:
				break;
				}
			}
		}
		public static void TODORENAME(){
			ButtonPanel menu = ButtonPanel.Create(26,60,0,190);
			ButtonPanel bp = ButtonPanel.Create(5*Player.minions.Count,30,20,200);
			MouseUI.RemoveButton(bp);
			menu.MainButton.AddButton(bp);
			menu.MainButton.nonblocking = true;
			bp.MainButton.draggable = true;
			bp.MainButton.locked_horizontal = true;
			Color4 color = Color4.Black.ChangeAlpha(0.84f);
			bp.Clear(color);
			menu.Clear(Color4.SlateGray);
			//bp.Clear(Color4.NavajoWhite.ChangeAlpha(0.7f));
			int idx = 0;
			foreach(Unit u in Player.minions){
				bp.Write(idx*5,8,u.name,Color4.White,color);
				bp.Write(idx*5+1,8,"".PadRight(u.health/2,(char)21),Color4.Red,color);
				bp.Write(idx*5+2,8,"".PadRight(u.initiative/2,(char)20),Color4.Lime,color);
				bp.Write(idx*5+3,8,"".PadRight(u.movement,(char)19),Color4.Cyan,color);
				bp.Write(idx*5+4,0,"".PadRight(30,'-'),Color4.Black,color);
				var sprite = UI.CreateSingleSprite(u,0,0);
				var button = new ButtonImage(Command.None,-1,0,0,80,50,sprite.ToArray());
				button.MoveTo(bp.MainButton.rect.X,bp.MainButton.rect.Y + idx*125);
				bp.MainButton.AddButton(button);
				idx++;
			}
			G.GetCommand();
			menu.Remove();
			bp.Remove();
		}
		public static List<Surface> CreateSingleSprite(Unit u,int v_offset_px,int h_offset_px){ //todo rename, createunitsprite or something
			bool undead = u.species != Species.Human;
			string filename = undead? "undead.png" : "living.png";
			Surface ss = Surface.Create(G.Window,filename,2);
			ss.texture = undead? G.undead.texture : G.living.texture;
			CellLayout.CreateGrid(ss,1,1,108,88,v_offset_px,h_offset_px);
			ss.SetEasyLayoutCounts(1);
			ss.SetDefaultSpriteType(0);
			ss.SetDefaultPosition(0);
			ss.SetDefaultLayout(0);
			if(undead){
				ss.UpdateMethod = d => {
					d.sprites = new List<int>{G.frame + ((G.Timer.Elapsed.Seconds/3)%4)*4 + u.species.idx*16};
				};
			}
			else{
				ss.UpdateMethod = d => {
					d.sprites = new List<int>(){((G.Timer.Elapsed.Seconds/3)%4) + u.human_type*4};
					//d.sprites = new List<int>{G.frame +  + u.species.idx*16};
				};
			}
			Surface hs = Surface.Create(G.Window,"living.png",2);
			hs.texture = G.living.texture;
			CellLayout.Create(hs,108,88,v_offset_px,h_offset_px,
				idx => {
					return ss.layouts[0].X(idx) + G.hats[u.species.idx,u.job.idx,(G.Timer.Elapsed.Seconds/3)%4,G.frame].col;
				},
				idx => {
					return ss.layouts[0].Y(idx) + G.hats[u.species.idx,u.job.idx,(G.Timer.Elapsed.Seconds/3)%4,G.frame].row;
				});
			hs.SetEasyLayoutCounts(1);
			hs.SetDefaultSpriteType(1);
			hs.SetDefaultPosition(0);
			hs.SetDefaultLayout(0);
			hs.UpdateMethod = d => {
				d.sprites = new List<int>{u.job.idx * 8 + ((G.Timer.Elapsed.Seconds/3)%4)};
			};
			ss.Update();
			hs.Update();
			G.FourPerSecondUpdateMethods.Add(ss.Update);
			G.FourPerSecondUpdateMethods.Add(hs.Update);
			return new List<Surface>{ss,hs};
		}
		public static void UpdateSingleSprite(List<Surface> list){
			foreach(Surface s in list){
				s.Update();
			}
		}
		public static void RemoveSingleSprite(List<Surface> list){
			foreach(Surface s in list){
				s.RemoveFromWindow();
				G.FourPerSecondUpdateMethods.Remove(s.Update);
			}
		}
	}
}
