//
//main -> set up GL window & stuff, then go to game menu or game itself ->
//
//generate an encounter & battle map -> start the battle
//
using System;
using System.Collections.Generic;
using GLDrawing;
using Utilities;
using PosArrays;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Bane;
namespace Bane{
	public static class BaneMain{
		public static void Main(string[] args){
			ToolkitOptions.Default.EnableHighResolution = false;
			U.DefaultMetric = DistanceMetric.Manhattan;
			G.Window = new GLWindow(1000,650,"Bane of the Living");
			G.Window.ResizingPreference = ResizeOption.AddBorder;
			G.Window.Mouse.Move += MouseUI.MouseMoveHandler;
			G.Window.Mouse.ButtonUp += MouseUI.MouseUpHandler;
			G.Window.Mouse.ButtonDown += MouseUI.MouseDownHandler;
			G.Window.Mouse.WheelChanged += MouseUI.MouseWheelHandler;
			G.Window.MouseLeave += MouseUI.MouseLeaveHandler;
			G.Window.KeyDown += (sender,keyargs) => {
				if(!G.CommandEntered){
					G.LastCommand = G.CommandFromKey(keyargs.Key);
					if(G.LastCommand != Command.Modifier){
						G.CommandEntered = true;
					}
					/*ConsoleKey ck = G.GetConsoleKey(keyargs.Key);
					if(ck != ConsoleKey.NoName){
						bool alt = G.Window.KeyIsDown(Key.LAlt) || G.Window.KeyIsDown(Key.RAlt);
						bool shift = G.Window.KeyIsDown(Key.LShift) || G.Window.KeyIsDown(Key.RShift);
						bool ctrl = G.Window.KeyIsDown(Key.LControl) || G.Window.KeyIsDown(Key.RControl);
						G.KeyPressed = true;
						G.LastKey = new ConsoleKeyInfo(G.GetChar(ck,shift),ck,shift,alt,ctrl);
					}*/
				}
			};
			G.terrain = Surface.Create(G.Window,"Terrain.png",Shader.NewTintFS(),true,2,4,4); //todo: update surface definitions to use global spacing vars.
			SpriteType.DefineSingleRowSprite(G.terrain,96);
			CellLayout.CreateIso(G.terrain,G.ROWS,G.COLS,257,96,0,0,24,48,idx => {
				int row = idx / G.COLS;
				int col = idx % G.COLS;
				return 0.99f - (float)(row+col) / ((float)(G.COLS) - 0.5f); //todo: maybe -0.5f should be a percentage instead
			},idx => {
				int row = idx / G.COLS;
				int col = idx % G.COLS;
				return -G.elevation_px * M.TileFromDrawingPosition(row,col).elevation;
			});
			CellLayout.CreateIsoAtOffset(G.terrain,1,G.COLS,8,0,8,257,96,0,0,24,48,idx=>-1,idx => -G.elevation_px * M.cutoff_elevation);
			CellLayout.CreateIsoAtOffset(G.terrain,G.ROWS,1,0,8,8,257,96,0,0,24,48,idx=>-1,idx => -G.elevation_px * M.cutoff_elevation);
			//G.terrain.SetOffsetInPixels(7,40);
			/*G.terrain.layouts[0].Y = idx => {
				int row = idx / G.COLS;
				int col = idx % G.COLS;
				return (row + col) * 24 - G.elevation_px * M.TileFromDrawingPosition(row,col).elevation;
			};*/
			G.terrain.SetEasyLayoutCounts(G.ROWS * G.COLS,G.COLS,G.ROWS); //The extra ROWS & COLS sprites are the pure black ones that make the bottom of the map appear to be even.
			//G.terrain.SetDefaultSprite(0);
			G.terrain.SetDefaultSpriteType(0);
			//G.terrain.SetDefaultOtherData(new List<float>{1.0f,1.0f,1.0f,1.0f});
			G.terrain.UpdateMethod = d => {
				d.other_data = new List<float>[]{new List<float>(),new List<float>()};
				d.sprites = new List<int>();
				for(int i=0;i<G.ROWS;++i){
					for(int j=0;j<G.COLS;++j){
						Tile t = M.TileFromDrawingPosition(i,j);
						//pos p = M.GetDrawingPosition(new pos(i,j));
						//int idx = U.Get1DIndex(p.row,p.col,G.COLS);
						if(t.super_todo_invisible){
							G.AddDefaultTint(d.other_data[0],d.other_data[1]);
							d.sprites.Add(2); //todo: clean this up.
							continue;
							G.AddInvisible(d.other_data[0],d.other_data[1]);
							continue; //todo fix
						}
						d.sprites.Add(0);
						if(t.grayed_out){
							G.AddGrayedOut(d.other_data[0],d.other_data[1]);
						}
						else{
							if(t.super_todo_extra_highlighted){
								G.AddHighlightBlue(d.other_data[0],d.other_data[1]);
							}
							else{
								if(t.highlighted){
									G.AddHighlightGreen(d.other_data[0],d.other_data[1]);
								}
								else{
									G.AddDefaultTint(d.other_data[0],d.other_data[1]);
								}
							}
						}
					}
				}
				for(int i=0;i<G.ROWS + G.COLS;++i){
					d.sprites.Add(3);
					G.AddDefaultTint(d.other_data[0],d.other_data[1]);
				}
				d.fill_count = d.sprites.Count;
				//d.fill_count = d.other_data[0].Count / 4;
			};
			G.living = Surface.Create(G.Window,"living.png",Shader.NewTintFS(),true,2,4,4);
			SpriteType.DefineSpriteDown(G.living,88,108,16);
			SpriteType.DefineSpriteAcross(G.living,88,108,8,7*88,0);
			CellLayout.CreateIso(G.living,G.ROWS,G.COLS,108,88,0,0,G.tile_spacing_vertical,G.tile_spacing_horizontal,idx => {
				int row = idx / G.COLS;
				int col = idx % G.COLS;
				return 0.985f - (float)(row+col) / ((float)(G.COLS) - 0.5f);
			},idx => {
				int row = idx / G.COLS;
				int col = idx % G.COLS;
				return -G.elevation_px * M.TileFromDrawingPosition(row,col).elevation;
			});
			CellLayout.CreateIso(G.living,G.ROWS,G.COLS,108,88,0,0,G.tile_spacing_vertical,G.tile_spacing_horizontal,idx => {
				int row = idx / G.COLS;
				int col = idx % G.COLS;
				return 0.98f - (float)(row+col) / ((float)(G.COLS) - 0.5f);
			});
			//G.living.SetOffsetInPixels(11,-21);
			/*G.living.layouts[0].Y = idx => {
				int row = idx / G.COLS;
				int col = idx % G.COLS;
				return (row + col) * G.tile_spacing_vertical - G.elevation_px * M.TileFromDrawingPosition(row,col).elevation; //todo: the elevation_px here should be part of the CreateIso call.
			};*/
			G.living.layouts[1].X = idx => {
				int row = idx / G.COLS;
				int col = idx % G.COLS;
				Actor a = M.ActorFromDrawingPosition(row,col);
				return (G.ROWS - 1 - row + col) * G.tile_spacing_horizontal + G.hats[a.base_unit.species.idx,a.base_unit.job.idx,a.RotatedFacing(),G.frame].col;
			};
			G.living.layouts[1].Y = idx => {
				int row = idx / G.COLS;
				int col = idx % G.COLS;
				Actor a = M.ActorFromDrawingPosition(row,col);
				return (row + col) * G.tile_spacing_vertical - G.elevation_px * M.TileFromDrawingPosition(row,col).elevation + G.hats[a.base_unit.species.idx,a.base_unit.job.idx,a.RotatedFacing(),G.frame].row; //todo, elevation_px, etc.
			};
			G.living.UpdateMethod = d => {
				d.positions = new List<int>();
				d.layouts = new List<int>();
				d.sprites = new List<int>();
				d.sprite_types = new List<int>();
				d.other_data = new List<float>[]{new List<float>(),new List<float>()};
				for(int i=0;i<G.ROWS;++i){
					for(int j=0;j<G.COLS;++j){
						Actor ac = M.ActorFromDrawingPosition(i,j);
						//pos p = M.GetDrawingPosition(new pos(i,j));
						int idx = U.Get1DIndex(i,j,G.COLS);
						//int idx = U.Get1DIndex(p.row,p.col,G.COLS);
						if(ac != null){
							if(ac.base_unit.species == Species.Human || ac.base_unit.species == Species.Shade){
								d.positions.Add(idx);
								d.layouts.Add(0);
								d.sprites.Add(ac.RotatedFacing() + 4 * ac.base_unit.human_type);
								d.sprite_types.Add(0);
								if(ac.grayed_out){
									G.AddGrayedOut(d.other_data[0],d.other_data[1]);
								}
								else{
									if(ac.highlighted){
										G.AddHighlightGreen(d.other_data[0],d.other_data[1]);
									}
									else{
										G.AddDefaultTint(d.other_data[0],d.other_data[1]);
									}
								}
								if(ac.base_unit.species == Species.Shade){
									d.other_data[0][d.other_data[0].Count-1] = 0.4f; //todo: maybe clean this up so it doesn't need to go back in to change to 0.4?
								}
							}
							if(ac.base_unit.job != Job.Commoner){
								d.positions.Add(idx);
								d.layouts.Add(1);
								d.sprites.Add(ac.base_unit.job.idx * 8 + ac.RotatedFacing());
								d.sprite_types.Add(1);
								if(ac.grayed_out){
									G.AddGrayedOut(d.other_data[0],d.other_data[1]);
								}
								else{
									if(ac.highlighted){
										G.AddHighlightGreen(d.other_data[0],d.other_data[1]);
									}
									else{
										G.AddDefaultTint(d.other_data[0],d.other_data[1]);
									}
								}
							}
						}
					}
				}
				//d.fill_count = d.positions.Count;
			};
			G.undead = Surface.Create(G.Window,"undead.png",Shader.NewTintFS(),true,2,4,4);
			SpriteType.DefineSpriteDown(G.undead,88,108,16);
			G.undead.layouts.Add(G.living.layouts[0]);
			//G.undead.SetOffsetInPixels(11,-21);
			G.undead.SetDefaultLayout(0);
			G.undead.SetDefaultSpriteType(0);
			//G.undead.SetDefaultOtherData(new List<float>{1.0f,1.0f,1.0f,1.0f});
			G.undead.UpdateMethod = d => {
				d.positions = new List<int>();
				d.sprites = new List<int>();
				d.other_data = new List<float>[]{new List<float>(),new List<float>()};
				for(int i=0;i<G.ROWS;++i){
					for(int j=0;j<G.COLS;++j){
						Actor ac = M.ActorFromDrawingPosition(i,j);
						//pos p = M.GetDrawingPosition(new pos(i,j));
						int idx = U.Get1DIndex(i,j,G.COLS);
						//int idx = U.Get1DIndex(p.row,p.col,G.COLS); //todo clean this up
						if(ac != null && ac.base_unit.species != Species.Human && ac.base_unit.species != Species.Shade){
							d.positions.Add(idx);
							d.sprites.Add(G.frame + ac.RotatedFacing()*4 + ac.base_unit.species.idx*16);
							if(ac.grayed_out){
								G.AddGrayedOut(d.other_data[0],d.other_data[1]);
							}
							else{
								if(ac.highlighted){
									G.AddHighlightGreen(d.other_data[0],d.other_data[1]);
								}
								else{
									G.AddDefaultTint(d.other_data[0],d.other_data[1]);
								}
							}
						}
					}
				}
				d.fill_count = d.positions.Count;
			};
			M.MapOffset = new pos(0,0);
			M.CreateMap();
			G.LoadHats();
			//M.DebugCreateRandomActors();
			G.terrain.Update();
			G.living.Update();
			G.undead.Update();
			G.Timer.Start();
			Surface t2 = Surface.Create(G.Window,"animations.png",2);
			SpriteType.DefineSingleRowSprite(t2,3);
			CellLayout.CreateGrid(t2,12,12,5,3,0,0);
			t2.SetOffsetInPixels(320,200);
			t2.SetDefaultSprite(0);
			t2.SetDefaultSpriteType(0);
			t2.SetEasyLayoutCounts(144);
			t2.DefaultUpdateOtherData();
			t2.layouts[0].X = idx => (idx % 7) * 3 + R.Between(-1,1);
			t2.layouts[0].Y = idx => (idx / 7) * 5 + R.Between(-1,1);
			float[] fx = new float[144];
			float[] fy = new float[144];
			int[] angle = new int[144];
			for(int i=0;i<144;++i){
				fx[i] = t2.layouts[0].X(i);
				fy[i] = t2.layouts[0].Y(i);
				angle[i] = R.Between(0,359);
			}
			t2.layouts[0].X = idx => {
				angle[idx] += R.Between(-25,25);
				fx[idx] += (float)Math.Sin(angle[idx] * Math.PI / 180.0);
				return fx[idx];
			};
			t2.layouts[0].Y = idx => {
				fy[idx] += (float)Math.Cos(angle[idx] * Math.PI / 180.0);
				return fy[idx];
			};
			t2.DefaultUpdatePositions();
			t2.Disabled = true;
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha,BlendingFactorDest.OneMinusSrcAlpha);
			GL.DepthFunc(DepthFunction.Less);
			G.Window.Visible = true;
			//G.Window.ResizingPreference = ResizeOption.AddBorder;
			G.Window.SnapWidth = 640; //todo remove
			G.Window.SnapHeight = 400;
			G.Window.EnforceRatio = true;
			for(int i=0;i<2;++i){
				Player.minions.Add(Unit.Create(0,Species.Skeleton,Job.Rogue,null));
				Player.minions.Add(Unit.Create(0,Species.Zombie,Job.Commoner,null));
			}
			G.FourPerSecondUpdateMethods.Add(G.living.Update);
			G.FourPerSecondUpdateMethods.Add(G.undead.Update);
			G.EveryFrameUpdateMethods.Add(t2.DefaultUpdatePositions);
			MouseUI.PushMode(InputMode.ActionSelect);
			while(true){
				if(G.GetCommand() == Command.Debug1){
					M.Rotation = (M.Rotation + 1).Modulo(4);
					G.terrain.Update();
					G.living.Update();
					G.undead.Update();
				}
				else{
					//G.FourPerSecondUpdateMethods.Remove(G.living.Update);
					//G.FourPerSecondUpdateMethods.Remove(G.undead.Update);
					G.EveryFrameUpdateMethods.Remove(t2.DefaultUpdatePositions);
					break;
				}
			}
			MainMenu();
		}
		public static void MainMenu(){
			MouseUI.PushMode(InputMode.ActionSelect); //todo: this push is unmatched!
			while(true){
				G.enemy_team = Unit.GetUnitsFromEncounterList(Unit.GetEncounterUnitList(G.battle++));
				G.foes = new List<Actor>();
				foreach(Unit u in G.enemy_team){
					G.foes.Add(Actor.Create(u));
				}
				G.todo_team = new List<Actor>();
				foreach(Unit u in Player.minions){
					G.todo_team.Add(Actor.Create(u));
				}
				Battle();
				ResolveBattle();
				Player.BetweenBattleMenu();
			}
		}
		public static void Battle(){
			//todo: WHEN I have a main menu, map-creation and surface management will happen here. Not until then.
			//M.CreateMap();
			PlaceUnitsByInitiative(G.foes,0);
			PlaceUnitsByInitiative(G.todo_team,2);
			new Event(EventType.NewTurn,0,21).Execute(); //todo: improve this later?
			while(true){
				Q.Pop();
				//todo check for victory
			}
		}
		public static void ResolveBattle(){
			//todo: this is where permanent changes to units happen, based on Actor statuses. maybe.
		}
		public static void PlaceUnitsByInitiative(List<Actor> units,int corner){
			List<Actor> ordered_units = new List<Actor>(units);
			ordered_units.Sort((f1,f2) => {
				int init_order = f1.initiative.CompareTo(f2.initiative);
				if(init_order == 0){
					return units.IndexOf(f1).CompareTo(units.IndexOf(f2)); //lower index in 'units', lower index in this list.
				}
				return -init_order; //lower initiative, higher index in this list.
			});
			int placement_n = 0;
			int p_row = 0;
			while(ordered_units.Count > 0){
				Actor ac = ordered_units.RemoveLast();
				pos p = new pos(p_row,placement_n - p_row).Rotate(corner);
				M.actor[p] = ac;
				ac.p = p;
				++p_row;
				if(p_row > placement_n){
					++placement_n;
					p_row = 0;
				}
			}
		}
	}
}
