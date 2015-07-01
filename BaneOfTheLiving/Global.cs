//
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Diagnostics;
using OpenTK.Input;
using OpenTK.Graphics;
using PosArrays;
using Utilities;
using Attributes;
using GLDrawing;
namespace Bane{
	public enum Command{None,Enter,Escape,Up,Down,Left,Right,Click,Debug1,Modifier,Selection};
	public static class G{
		public static readonly int ROWS = 8;
		public static readonly int COLS = 8;
		public static readonly int elevation_px = 16;
		public static readonly int tile_h = 48;
		public static readonly int tile_w = 96;
		public static readonly int tile_spacing_vertical = 24;
		public static readonly int tile_spacing_horizontal = 48;
		public static int battle = 1;
		public static List<Unit> enemy_team = null;
		public static List<Actor> foes = null;
		public static List<Actor> todo_team = null; //todo: TODO!! Seriously, reorganize this. The units are in Player and the rest are here, why?
		public static bool player_has_initiative_advantage = true; //todo: find a better spot for this.
		public static int active_team = -1;
		public static GLWindow Window;
		public static Surface terrain;
		public static Surface undead;
		public static Surface living; //and hats
		public static pos[,,,] hats;
		public static Stopwatch Timer = new Stopwatch(); //isn't this capitalization scheme great?
		public static List<Action> EveryFrameUpdateMethods = new List<Action>();
		public static List<Action> FourPerSecondUpdateMethods = new List<Action>(); //I could turn this into a more general timing system...eventually.
		public static int frame = 0; //todo: capitalization?

		public static bool CommandEntered = false;
		public static Command LastCommand;
		public static ConsoleKeyInfo LastKey; 
		public static Tile LastTile; //for mouse input
		public static int LastSelection;
		public static Command GetCommand(){
			while(true){
				if(!G.Window.WindowUpdate()){
					Environment.Exit(0);
				}
				int new_frame = G.Timer.Elapsed.Milliseconds / 250;
				if(frame != new_frame){
					frame = new_frame;
					foreach(Action a in FourPerSecondUpdateMethods){
						a();
					}
				}
				foreach(Action a in EveryFrameUpdateMethods){
					a();
				}
				Thread.Sleep(10);
				if(CommandEntered){
					MouseUI.ClearHighlights();
					CommandEntered = false;
					return LastCommand;
				}
			}
		}
		public static Command CommandFromKey(Key key){
			switch(key){
			case Key.Escape:
			return Command.Escape;
			case Key.Enter:
			case Key.KeypadEnter:
			return Command.Enter;
			case Key.Up:
			case Key.Keypad8:
			case Key.PageUp:
			case Key.Keypad9:
			return Command.Up;
			case Key.Down:
			case Key.Keypad2:
			case Key.End:
			case Key.Keypad1:
			return Command.Down;
			case Key.Left:
			case Key.Keypad4:
			case Key.Home:
			case Key.Keypad7:
			return Command.Left;
			case Key.Right:
			case Key.Keypad6:
			case Key.PageDown:
			case Key.Keypad3:
			return Command.Right;
			case Key.A:
			return Command.Debug1;
			case Key.LShift:
			case Key.RShift:
			case Key.LAlt:
			case Key.RAlt:
			case Key.LControl:
			case Key.RControl:
			return Command.Modifier;
			default:
			return Command.None;
			}
		}
		public static int RotatedDirectionFromInput(Command command){
			switch(command){
			case Command.Up:
			return 8.RotateFourWayDir(false,M.Rotation);
			case Command.Down:
			return 2.RotateFourWayDir(false,M.Rotation);
			case Command.Left:
			return 4.RotateFourWayDir(false,M.Rotation);
			case Command.Right:
			default:
			return 6.RotateFourWayDir(false,M.Rotation);
			}
		}
		public static int TrueDirectionFromScreenDirection(int screen_direction){
			return screen_direction.RotateFourWayDir(false,M.Rotation);
		}
		public static int ScreenDirectionFromTrueDirection(int true_direction){
			return true_direction.RotateFourWayDir(true,M.Rotation);
		}
		/*public static ConsoleKey GetConsoleKey(Key key){
			if(key >= Key.A && key <= Key.Z){
				return (ConsoleKey)(key - (Key.A - (int)ConsoleKey.A));
			}
			if(key >= Key.Number0 && key <= Key.Number9){
				return (ConsoleKey)(key - (Key.Number0 - (int)ConsoleKey.D0));
			}
			if(key >= Key.Keypad0 && key <= Key.Keypad9){
				return (ConsoleKey)(key - (Key.Keypad9 - (int)ConsoleKey.NumPad9));
			}
			if(key >= Key.F1 && key <= Key.F12){
				return (ConsoleKey)(key - (Key.F1 - (int)ConsoleKey.F1));
			}
			switch(key){
			case Key.BackSpace:
			return ConsoleKey.Backspace;
			case Key.Tab:
			return ConsoleKey.Tab;
			case Key.Enter:
			case Key.KeypadEnter:
			return ConsoleKey.Enter;
			case Key.Escape:
			return ConsoleKey.Escape;
			case Key.Space:
			return ConsoleKey.Spacebar;
			case Key.Delete:
			return ConsoleKey.Delete;
			case Key.Up:
			return ConsoleKey.UpArrow;
			case Key.Down:
			return ConsoleKey.DownArrow;
			case Key.Left:
			return ConsoleKey.LeftArrow;
			case Key.Right:
			return ConsoleKey.RightArrow;
			case Key.Comma:
			return ConsoleKey.OemComma;
			case Key.Period:
			return ConsoleKey.OemPeriod;
			case Key.Minus:
			return ConsoleKey.OemMinus;
			case Key.Plus:
			return ConsoleKey.OemPlus;
			case Key.Tilde:
			return ConsoleKey.Oem3;
			case Key.BracketLeft:
			return ConsoleKey.Oem4;
			case Key.BracketRight:
			return ConsoleKey.Oem6;
			case Key.BackSlash:
			return ConsoleKey.Oem5;
			case Key.Semicolon:
			return ConsoleKey.Oem1;
			case Key.Quote:
			return ConsoleKey.Oem7;
			case Key.Slash:
			return ConsoleKey.Oem2;
			case Key.Home:
			return ConsoleKey.Home;
			case Key.End:
			return ConsoleKey.End;
			case Key.PageUp:
			return ConsoleKey.PageUp;
			case Key.PageDown:
			return ConsoleKey.PageDown;
			case Key.Clear:
			return ConsoleKey.Clear;
			case Key.Insert:
			return ConsoleKey.Insert;
			default:
			return ConsoleKey.NoName;
			} //(for the record, the numpad symbols are Add/Divide/Multiply/Subtract, the decimal/delete depends on numlock, and 0/insert and 5/clear do too.)
		}
		public static char GetChar(ConsoleKey k,bool shift){
			if(k >= ConsoleKey.A && k <= ConsoleKey.Z){
				if(shift){
					return k.ToString()[0];
				}
				else{
					return k.ToString().ToLower()[0];
				}
			}
			if(k >= ConsoleKey.D0 && k <= ConsoleKey.D9){
				if(shift){
					switch(k){
					case ConsoleKey.D1:
					return '!';
					case ConsoleKey.D2:
					return '@';
					case ConsoleKey.D3:
					return '#';
					case ConsoleKey.D4:
					return '$';
					case ConsoleKey.D5:
					return '%';
					case ConsoleKey.D6:
					return '^';
					case ConsoleKey.D7:
					return '&';
					case ConsoleKey.D8:
					return '*';
					case ConsoleKey.D9:
					return '(';
					case ConsoleKey.D0:
					default:
					return ')';
					}
				}
				else{
					return k.ToString()[1];
				}
			}
			if(k >= ConsoleKey.NumPad0 && k <= ConsoleKey.NumPad9){
				return k.ToString()[6];
			}
			switch(k){
			case ConsoleKey.Tab:
			return (char)9;
			case ConsoleKey.Enter:
			return (char)13;
			case ConsoleKey.Escape:
			return (char)27;
			case ConsoleKey.Spacebar:
			return ' ';
			case ConsoleKey.OemComma:
			if(shift){
				return '<';
			}
			else{
				return ',';
			}
			case ConsoleKey.OemPeriod:
			if(shift){
				return '>';
			}
			else{
				return '.';
			}
			case ConsoleKey.OemMinus:
			if(shift){
				return '_';
			}
			else{
				return '-';
			}
			case ConsoleKey.OemPlus:
			if(shift){
				return '+';
			}
			else{
				return '=';
			}
			case ConsoleKey.Oem3:
			if(shift){
				return '~';
			}
			else{
				return '`';
			}
			case ConsoleKey.Oem4:
			if(shift){
				return '{';
			}
			else{
				return '[';
			}
			case ConsoleKey.Oem6:
			if(shift){
				return '}';
			}
			else{
				return ']';
			}
			case ConsoleKey.Oem5:
			if(shift){
				return '|';
			}
			else{
				return '\\';
			}
			case ConsoleKey.Oem1:
			if(shift){
				return ':';
			}
			else{
				return ';';
			}
			case ConsoleKey.Oem7:
			if(shift){
				return '"';
			}
			else{
				return '\'';
			}
			case ConsoleKey.Oem2:
			if(shift){
				return '?';
			}
			else{
				return '/';
			}
			default:
			return (char)0;
			}
		}*/
		public static void LoadHats(){
			int num_dudes = 38;
			int num_hats = 17;
			int num_frames = 4;
			hats = new pos[num_dudes,num_hats,4,num_frames]; //4 facings
			FileStream file = new FileStream("hatdata",FileMode.Open);
			BinaryReader r = new BinaryReader(file);
			for(int a=0;a<num_dudes;++a){
				for(int b=0;b<num_hats;++b){
					for(int c=0;c<4;++c){
						for(int d=0;d<num_frames;++d){
							int col = r.ReadInt32();
							int row = r.ReadInt32();
							hats[a,b,c,d] = new pos(row,col);
						}
					}
				}
			}

			r.Close();
			file.Close();
		}
		public static void AddDefaultTint(List<float> multiply_list,List<float> add_list){
			multiply_list.AddRange(new float[]{1.0f,1.0f,1.0f,1.0f});
			add_list.AddRange(new float[]{0.0f,0.0f,0.0f,0.0f});
		}
		public static void AddGrayedOut(List<float> multiply_list,List<float> add_list){
			multiply_list.AddRange(new float[]{0.5f,0.5f,0.5f,1.0f});
			add_list.AddRange(new float[]{0.0f,0.0f,0.0f,0.0f});
		}
		public static void AddHighlightRed(List<float> multiply_list,List<float> add_list){
			multiply_list.AddRange(new float[]{0.5f,1.0f,1.0f,1.0f});
			add_list.AddRange(new float[]{0.5f,0.0f,0.0f,0.0f});
		}
		public static void AddHighlightGreen(List<float> multiply_list,List<float> add_list){
			multiply_list.AddRange(new float[]{1.0f,0.5f,1.0f,1.0f});
			add_list.AddRange(new float[]{0.0f,0.5f,0.0f,0.0f});
		}
		public static void AddHighlightBlue(List<float> multiply_list,List<float> add_list){
			multiply_list.AddRange(new float[]{1.0f,1.0f,0.5f,1.0f});
			add_list.AddRange(new float[]{0.0f,0.0f,0.5f,0.0f});
		}
		public static void AddInvisible(List<float> multiply_list,List<float> add_list){
			multiply_list.AddRange(new float[]{1.0f,1.0f,1.0f,0.4f});
			add_list.AddRange(new float[]{0.0f,0.0f,0.0f,0.0f});
		}
		public static Color4 ChangeAlpha(this Color4 color,float new_alpha){ //todo: could use darken/lighten methods, too.
			return new Color4(color.R,color.G,color.B,new_alpha);
		}
		public static System.Drawing.Point ConvertToPoint(this pos p){
			return new System.Drawing.Point(p.col,p.row);
		}
		public static pos Rotate(this pos p,int times){
			times = times % 4;
			switch(times){
			case 0:
			return p;
			case 1:
			return new pos(p.col,COLS-1 - p.row);
			case 2:
			return new pos(ROWS-1 - p.row,COLS-1 - p.col);
			case 3:
			default:
			return new pos(ROWS-1 - p.col,p.row);
			}
		}
		public static int DistanceFrom(this pos p,pos dest){
			return p.ManhattanDistanceFrom(dest);
		}
		public static int DistanceFrom(this pos p,PhysicalObject dest){
			return p.ManhattanDistanceFrom(dest.row,dest.col);
		}
		public static int DistanceFrom(this pos p,int r,int c){
			return p.ManhattanDistanceFrom(r,c);
		}
		public static Attribute<Attr> Ready(this Attr a,int value,object source,Time time){
			return new Attribute<Attr>(a,value,source,time);
		}
		public static Attribute<Attr> Ready(this Attr a,object source,Time time){
			return new Attribute<Attr>(a,1,source,time);
		}
		public static Attribute<Attr> Ready(this Attr a,int value,object source){
			return new Attribute<Attr>(a,value,source);
		}
		public static Attribute<Attr> Ready(this Attr a,object source){
			return new Attribute<Attr>(a,1,source);
		}
		public static string Name(this Attr a){
			string s = a.ToString();
			string result = s[0].ToString();
			for(int i=1;i<s.Length;++i){
				if(s[i] >= 'A' && s[i] <= 'Z'){
					result += " ";
				}
				result += s[i];
			}
			return result;
		}
	}
}
