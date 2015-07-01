//
using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK.Input;
using OpenTK.Graphics;
using GLDrawing;
using PosArrays;
using TextPanels;
using Utilities;
namespace Bane{
	public enum InputMode{ActionSelect,MoveAction,Targeting};
	public class DragTracker{
		public IMouseTarget target;
		public bool dragging;
		public Point start_location;
		public Point click_offset_from_target_origin;
		public Action<int,int> update;
		public Rectangle perimeter; //the dragged object won't go past the perimeter. A width or height of 0 means unrestricted movement in that dimension.
		public bool locked_horizontal;
		public bool locked_vertical;
	}
	public static class MouseUI{
		public static DragTracker dragTarget = null;
		/*public static bool potential_drag = false;
		public static Action<int,int> drag_update = null;
		public static bool dragging = false;
		public static Point drag_origin;
		//public static pos drag_delta;
		public static Point drag_rect_offset; //this value tracks *where* you clicked within the target rectangle.
		public static Point current_position; //todo: this might be better as "last position" and it might be better private*/
		private static List<List<Tile>> highlighted = new List<List<Tile>>();
		private static pos[,] click_offsets;
		private static LinkedList<InputMode> modestack = new LinkedList<InputMode>();
		private static LinkedList<List<IMouseTarget>> buttonstack = new LinkedList<List<IMouseTarget>>();
		static MouseUI(){
			click_offsets = new pos[G.tile_h,G.tile_w];
			int max = G.tile_w/2;
			for(int i=0;i<G.tile_h/2;++i){
				for(int j=0;j<max;++j){
					click_offsets[i,j] = new pos(0,-1); //upper left
					click_offsets[i,G.tile_w-1-j] = new pos(-1,0); //upper right
					click_offsets[G.tile_h-1-i,j] = new pos(1,0); //lower left
					click_offsets[G.tile_h-1-i,G.tile_w-1-j] = new pos(0,1); //lower right
				}
				max -= 2;
			}
		}
		public static bool ButtonClicked(Point p){
			var n = buttonstack.Last;
			if(n != null && n.Value != null){
				foreach(IMouseTarget b in n.Value){
					if(b.Clicked(p)){
						return true;
					}
				}
				/*foreach(Button b in n.Value){
					if(CheckButton(b,p)){
						return true;
					}
				}*/
			}
			return false;
		}
		/*private static bool CheckButton(Button b,Point p){
			if(b.rect.Contains(p)){
				if(b.sub_buttons != null){
					foreach(Button sub in b.sub_buttons){
						if(CheckButton(sub,p)){
							return true;
						}
					}
				}
				if(b.command != Command.None){
					G.LastCommand = b.command;
					G.LastSelection = b.selection;
					G.CommandEntered = true;
					return true;
				}
			}
			return false;
		}*/
		public static DragTracker CheckForDrag(Point p){
			var n = buttonstack.Last;
			if(n != null && n.Value != null){
				foreach(IMouseTarget b in n.Value){
					var d = b.CheckDrag(p);
					if(d != null){
						return d;
					}
				}
			}
			return null;
		}
		/*public static Action<int,int> GetDrag(Point p){
			var n = buttonstack.Last;
			if(n != null && n.Value != null){
				foreach(IMouseTarget b in n.Value){
					var d = b.OnDrag(p);
					if(d != null){
						//drag_rect_offset = new Point(
						return d;
					}
				}
			}
			return null;
		}
		private static Button CheckDraggableButton(Button b,Point p){
			if(b.rect.Contains(p)){
				if(b.sub_buttons != null){
					foreach(Button sub in b.sub_buttons){
						Button result = CheckDraggableButton(sub,p);
						if(result != null){
							return result;
						}
					}
				}
				if(b.draggable){
					return b;
				}
			}
			return null;
		}*/
		public static void AddButton(IMouseTarget b){
			if(buttonstack.Last.Value == null){
				buttonstack.Last.Value = new List<IMouseTarget>();
			}
			buttonstack.Last.Value.Add(b);
		}
		public static void RemoveButton(IMouseTarget b){
			if(buttonstack.Last.Value != null){
				buttonstack.Last.Value.Remove(b);
			}
		}
		public static void PushMode(InputMode mode){
			modestack.AddFirst(mode);
			buttonstack.AddFirst(new LinkedListNode<List<IMouseTarget>>(null));
		}
		public static void PopMode(){
			modestack.RemoveFirst();
			buttonstack.RemoveFirst();
		}
		public static void ClearHighlights(){
			if(highlighted.Count > 0){
				foreach(List<Tile> l in highlighted){
					foreach(Tile t in l){
						t.grayed_out = false;
						t.highlighted = false;
						t.super_todo_extra_highlighted = false;
						t.super_todo_invisible = false;
					}
				}
				highlighted.Clear();
				G.terrain.Update();
			}
		}
		private static pos GetExtendedClickOffset(int r,int c){
			if(click_offsets.BoundsCheck(r,c)){
				return click_offsets[r,c];
			}
			if(c >= click_offsets.GetLength(1) / 2){ //then we're on the right side
				if(r < 0){
					return new pos(-1,0);
				}
				return new pos(0,1);
			}
			else{ //left side
				if(r < 0){
					return new pos(0,-1);
				}
				return new pos(1,0);
			}
		}
		/*public static pos ClickedPosition(System.Drawing.Point mouse_pos){ //here's the flat isometric position finder.
			int tile_w = 48; //todo check, make a global var, something.
			int new_y = mouse_pos.Y - M.MapOffset.row - 40;
			int new_x = mouse_pos.X - ((G.ROWS-1) * tile_w + M.MapOffset.col + 6); //-40 and 6 are offsets chosen so the click would be as close as possible.
			int cell_row = new_y.IntDivide(48);
			int cell_col = new_x.IntDivide(96);
			int total = cell_row * 2; //'total' is the central tile's row+col.
			int diff = cell_col * -2; //'diff' is the central tile's row-col.
			int tile_col = (total - diff) / 2;
			int tile_row = tile_col + diff;
			int cell_internal_row = new_y.Modulo(48); //all these ints that are used once - definitely relying on the optimizer here.
			int cell_internal_col = new_x.Modulo(96);
			pos offset = click_offsets[cell_internal_row,cell_internal_col];
			return new pos(tile_row + offset.row,tile_col + offset.col);
		}*/
		public static Tile ClickedTile(Point mouse_pos){
			bool from_the_top = G.Window.KeyIsDown(Key.ControlLeft) || G.Window.KeyIsDown(Key.ControlRight);
			int new_y = mouse_pos.Y - M.MapOffset.row - 40;
			int new_x = mouse_pos.X - G.ROWS * G.tile_w / 2 - M.MapOffset.col - 6; //-40 and -6 are offsets chosen so the click will be as close as possible.
			int cell_col = new_x / (G.tile_w / 2); //todo: might be 1px off here because -48 will return -1, when it should maybe return 0.
			int current_row;
			int current_col;
			if(from_the_top){
				if(cell_col < 0){ //then we hit the col limit first
					current_col = 0;
					current_row = -cell_col;
				}
				else{
					current_row = 0;
					current_col = cell_col;
				}
			}
			else{
				if(G.ROWS - G.COLS < -cell_col){ //then we hit the row limit first
					current_row = G.ROWS - 1;
					current_col = current_row + cell_col;
				}
				else{
					current_col = G.COLS - 1;
					current_row = current_col - cell_col;
				}
			}
			while(true){
				Tile t = M.TileFromDrawingPosition(current_row,current_col);
				if(t == null){
					return null;
				}
				int elev_offset = t.elevation * G.elevation_px; //todo, rotate
				int tile_position_offset = (current_row + current_col) * G.tile_h / 2; //todo, use G.tile_spacing
				int cell_internal_row = (new_y + elev_offset - tile_position_offset);
				int cell_internal_col = (new_x - (cell_col%2 == 0? G.tile_w/2 : 0)).Modulo(G.tile_w);
				pos map_offset = GetExtendedClickOffset(cell_internal_row,cell_internal_col);
				int offset_total = map_offset.row + map_offset.col;
				if(offset_total == 0){
					return t; //great, it's the central tile.
				}
				if(from_the_top){
					if(offset_total == -1){
						map_offset = new pos(-map_offset.col,-map_offset.row); //don't try to go back up
					}
				}
				else{
					if(offset_total == 1){ //the downward directions indicate that we're clicking in empty space or on the side of a tile.
						return null;
					}
				}
				current_row += map_offset.row;
				current_col += map_offset.col;
				cell_col += map_offset.col - map_offset.row;
			}
		}
		public static List<Tile> BlockingTiles(Tile blocked){
			List<Tile> result = new List<Tile>();
			int threshold = blocked.elevation * G.elevation_px;
			pos p = M.GetDrawingPosition(blocked);
			Tile current = blocked;
			while(current != null){
				Tile other = current.TileInDirection(G.TrueDirectionFromScreenDirection(2));
				if(other != null && other.elevation * G.elevation_px > threshold){
					result.Add(other);
				}
				other = current.TileInDirection(G.TrueDirectionFromScreenDirection(6));
				if(other != null && other.elevation * G.elevation_px > threshold){
					result.Add(other);
				}
				p = new pos(p.row+1,p.col+1);
				current = M.TileFromDrawingPosition(p);
				if(current != null && current.elevation * G.elevation_px > threshold){
					result.Add(current);
				}
				threshold += G.tile_h;
			}
			return result;
		}
		public static void MouseMoveHandler(object sender,MouseMoveEventArgs args){
			if(dragTarget != null){
				if(!dragTarget.dragging && (Math.Abs(dragTarget.start_location.X - args.X) > 35 || Math.Abs(dragTarget.start_location.Y - args.Y) > 35)){
					dragTarget.dragging = true;
				}
				if(dragTarget.dragging && dragTarget.update != null){
					int new_x = args.X - dragTarget.click_offset_from_target_origin.X;
					int new_y = args.Y - dragTarget.click_offset_from_target_origin.Y;
					if(dragTarget.target != null){
						if(dragTarget.perimeter.Width > 0){
							if(new_x < dragTarget.perimeter.Left){
								new_x = dragTarget.perimeter.Left;
							}
							else{
								if(new_x + dragTarget.target.BoundingRect().Width > dragTarget.perimeter.Right){
									new_x = dragTarget.perimeter.Right - dragTarget.target.BoundingRect().Width;
								}
							}
						}
						if(dragTarget.perimeter.Height > 0){
							if(new_y < dragTarget.perimeter.Top){
								new_y = dragTarget.perimeter.Top;
							}
							else{
								if(new_y + dragTarget.target.BoundingRect().Height > dragTarget.perimeter.Bottom){
									new_y = dragTarget.perimeter.Bottom - dragTarget.target.BoundingRect().Height;
								}
							}
						}
					}
					if(dragTarget.locked_vertical){
						new_y = dragTarget.start_location.Y - dragTarget.click_offset_from_target_origin.Y; //todo: is this right? should it be based on 'target'?
					}
					if(dragTarget.locked_horizontal){
						new_x = dragTarget.start_location.X - dragTarget.click_offset_from_target_origin.X;
					}
					dragTarget.update(new_x,new_y);
				}
			}
			else{
				if(G.Window.KeyIsDown(Key.ControlLeft) || G.Window.KeyIsDown(Key.ControlRight)){
					Tile t = ClickedTile(args.Position);
					if(t != null){
						ClearHighlights();
						List<Tile> blocking = BlockingTiles(t);
						foreach(Tile t2 in blocking){
							t2.super_todo_invisible = true;
						}
						highlighted.Add(blocking);
						G.terrain.Update();
					}
				}
			}
		}
		public static void MouseDownHandler(object sender,MouseButtonEventArgs args){
			if(args.Button == MouseButton.Middle || args.Button == MouseButton.Right){
				return;
			}
			var d = CheckForDrag(args.Position);
			if(d != null){
				dragTarget = d;
			}
			else{
				bool map = true; // todo !
				if(map){
					dragTarget = M.GetMapDrag(args.Position);
				}
			}
		}
		public static void MouseUpHandler(object sender,MouseButtonEventArgs args){
			if(args.Button == MouseButton.Middle){
				HandleMiddleClick();
				return;
			}
			if(args.Button == MouseButton.Right){
				HandleRightClick();
				return;
			}
			bool dragging = (dragTarget != null && dragTarget.dragging);
			dragTarget = null;
			if(!dragging){
				if(!G.CommandEntered){
					if(ButtonClicked(args.Position)){
						return; //todo
					}
					Tile t = ClickedTile(args.Position);
					if(t != null){
						//t.super_todo_extra_highlighted = true;
						//G.terrain.Update();
						G.LastTile = t;
						G.LastCommand = Command.Click;
						G.CommandEntered = true;
					}
				}
			}
		}
		public static void HandleMiddleClick(){
		}
		public static void HandleRightClick(){
			if(dragTarget != null && dragTarget.dragging && dragTarget.update != null){
				//dragTarget.update(dragTarget.start_location.X,dragTarget.start_location.Y);
				dragTarget.update(dragTarget.start_location.X - dragTarget.click_offset_from_target_origin.X,dragTarget.start_location.Y - dragTarget.click_offset_from_target_origin.Y);
			}
			dragTarget = null;
		}
		public static void MouseWheelHandler(object sender,MouseWheelEventArgs args){
		}
		public static void MouseLeaveHandler(object sender,EventArgs args){
		}
	}
	public interface IMouseTarget{
		//Func<Point,bool> clicked{get;} //does a click at the given System.Drawing.Point generate a valid result for this target? if true, no more targets will be considered.
		//Func<Point,Action<int,int>> on_drag{get;} //dx and dy are supplied in pixels here.
		bool Clicked(Point p);
		void Move(int dx,int dy);
		void MoveTo(int x,int y);
		DragTracker CheckDrag(Point p);
		//Action<int,int> OnDrag(Point p);
		Rectangle BoundingRect();
	}
	public class Button : IMouseTarget{
		public Command command;
		public int selection;
		public Rectangle rect;
		public bool draggable;
		public bool locked_horizontal;
		public bool locked_vertical;
		public bool nonblocking; // if nonblocking is true, buttons inside this one can be dragged outside. Note that clicks on the portions beyond the perimeter will not be registered.
		public List<IMouseTarget> sub_buttons;
		public virtual bool Clicked(Point p){
			if(rect.Contains(p)){
				if(sub_buttons != null){
					foreach(IMouseTarget sub in sub_buttons){
						if(sub.Clicked(p)){
							return true;
						}
					}
				}
				if(command != Command.None){
					G.LastCommand = command;
					G.LastSelection = selection;
					G.CommandEntered = true;
					return true;
				}
			}
			return false;
		}
		public virtual DragTracker CheckDrag(Point p){
			if(rect.Contains(p)){
				if(sub_buttons != null){
					foreach(IMouseTarget sub in sub_buttons){
						var d = sub.CheckDrag(p);
						if(d != null){
							if(d.perimeter.IsEmpty && !nonblocking){ //the perimeter will be the first parent of the target.
								d.perimeter = rect;
							}
							return d;
						}
					}
				}
				if(draggable){
					DragTracker d = new DragTracker();
					d.target = this;
					d.update = MoveTo;
					d.start_location = p;
					d.click_offset_from_target_origin = new Point(p.X - rect.X,p.Y - rect.Y);
					d.locked_horizontal = locked_horizontal;
					d.locked_vertical = locked_vertical;
					return d;
				}
			}
			return null;
		}
/*							return (x,y) => {
								Rectangle subrect = sub.BoundingRect();
								if(x - offset.X < rect.Left){
									x = rect.Left + offset.X;
								}
								else{
									if(x - offset.X + subrect.Width > rect.Right){ //todo: wtf, should this be -1 or not?
										x = rect.Right - subrect.Width + offset.X;
									}
								}
								/*if(subrect.Left + dx < rect.Left){
									dx = rect.Left - subrect.Left;
								}
								else{
									if(subrect.Right + dx > rect.Right){
										dx = rect.Right - subrect.Right;
									}
								}
								if(subrect.Top + dy < rect.Top){		// NEXT:  this code below doesn't work yet; fix it. 
									dy = rect.Top - subrect.Top;
								}
								else{
									if(subrect.Bottom + dy > rect.Bottom){
										dy = rect.Bottom - subrect.Bottom;
									}
								}**
								if(MouseUI.current_position.X >= rect.Right || MouseUI.current_position.X <= rect.Left){ //todo: does origin + delta even work if the mouse goes offscreen? this probably requires getting an ACTUAL mouse position and passing it in.
								//if(MouseUI.drag_origin.X + MouseUI.drag_delta.col + dx >= rect.Right){ //todo: does origin + delta even work if the mouse goes offscreen? this probably requires getting an ACTUAL mouse position and passing it in.
									dx = 0;
								}
								if(MouseUI.current_position.Y >= rect.Bottom || MouseUI.current_position.Y <= rect.Top){ //todo: not quite right here yet, anyway.
								//if(MouseUI.drag_origin.Y + MouseUI.drag_delta.row + dy >= rect.Bottom){ //todo: not quite right here yet, anyway.
									dy = 0;
								}
								d(dx,dy);
							};
						}
					}
				}*/
		public Rectangle BoundingRect(){ return rect; }
		//public Func<Point,bool> clicked{ get{ return internal_clicked; } }
		//public Func<Point,Action<int,int>> on_drag{ get{ return internal_on_drag; } }
		public Button(Command command_,int v_offset_px,int h_offset_px,int height_px,int width_px){ //todo: allow different corners?
			command = command_;
			selection = -1;
			rect = new Rectangle(h_offset_px,v_offset_px,width_px,height_px);
			//drag_update = Move;
		}
		public Button(Command command_,int selection_,int v_offset_px,int h_offset_px,int height_px,int width_px){
			command = command_;
			selection = selection_;
			rect = new Rectangle(h_offset_px,v_offset_px,width_px,height_px);
			//drag_update = Move;
		}
		public void AddButton(IMouseTarget b){
			if(sub_buttons == null){
				sub_buttons = new List<IMouseTarget>();
			}
			sub_buttons.Add(b);
		}
		public void RemoveButton(IMouseTarget b){
			if(sub_buttons != null){
				sub_buttons.Remove(b);
			}
		}
		public virtual void Move(int dx,int dy){
			if(sub_buttons != null){
				foreach(IMouseTarget b in sub_buttons){
					b.Move(dx,dy);
					//b.rect = new Rectangle(b.rect.X + dx,b.rect.Y + dy,b.rect.Width,b.rect.Height);
				}
			}
			rect = new Rectangle(rect.X + dx,rect.Y + dy,rect.Width,rect.Height);
		}
		public virtual void MoveTo(int x,int y){
			if(sub_buttons != null){
				int dx = x - rect.X;
				int dy = y - rect.Y;
				foreach(IMouseTarget b in sub_buttons){
					b.Move(dx,dy);
					//b.MoveTo(x,y);
					//b.rect = new Rectangle(b.rect.X + dx,b.rect.Y + dy,b.rect.Width,b.rect.Height);
				}
			}
			rect = new Rectangle(x,y,rect.Width,rect.Height);
		}
	}
	public class ButtonImage : Button{
		public List<Surface> surfaces;
		public ButtonImage(Command command_,int selection_,int v_offset_px,int h_offset_px,int height_px,int width_px,params Surface[] surfaces_) : base(command_,selection_,v_offset_px,h_offset_px,height_px,width_px){
			surfaces = new List<Surface>(surfaces_); //todo: this needs a better constructor. Maybe it should find the layout and use its size.
		}
		public override void Move(int dx,int dy){
			foreach(Surface s in surfaces){
				s.ChangeOffsetInPixels(dx,dy);
			}
			base.Move(dx,dy);
		}
		public override void MoveTo(int x,int y){ //todo! Here's a problem with ButtonImage: It takes a list of surfaces, but the UI.SingleSprite stuff adds its animations to the update list.
			Move(x - rect.X,y - rect.Y); //just do the delta version so it moves the surfaces properly.
			/*foreach(Surface s in surfaces){ // ...so, removing it the usual way would miss those. What to do?
				s.SetOffsetInPixels(x,y);
			}
			base.MoveTo(x,y);*/
		}
	}
	public class ButtonPanel : TextPanel, IMouseTarget{
		/*private const int cell_w = 8;
		private const int cell_h = 16;
		private const string filename = "font8x16.bmp";
		private const int font_w = 8;
		private const int font_padding = 1;*/
		private const int cell_w = 10;
		private const int cell_h = 25;
		private const string filename = "Inconsolata_mod.png";
		private const int font_w = 10;
		private const int font_padding = 0;
		private const bool aa_font = true;
		protected PosArray<Button> buttons;
		protected ButtonImage internal_main_button = null;
		public ButtonImage MainButton{
			get{
				return internal_main_button; //todo: how about a ReplaceButton method, with a bool transfer_sub_buttons?
			}
			/*set{
				if(value == null){
					if(internal_main_button != null){
						if(internal_main_button.sub_buttons != null){
							foreach(Button b in internal_main_button.sub_buttons){
								MouseUI.AddButton(b);
							}
						}
						MouseUI.RemoveButton(internal_main_button);
						internal_main_button = null;
					}
				}
				else{
					if(internal_main_button != null){
						if(internal_main_button.sub_buttons != null){
							foreach(Button b in internal_main_button.sub_buttons){ //not removing them from the old one. Probably don't need to.
								value.sub_buttons.Add(b);
							}
						}
						MouseUI.RemoveButton(internal_main_button);
						MouseUI.AddButton(value);
					}
					else{
						foreach(Button b in buttonlist){
							MouseUI.RemoveButton(b);
							value.sub_buttons.Add(b);
						}
						MouseUI.AddButton(value);
					}
					internal_main_button = value;
				}
			}*/
		}
		protected ButtonPanel(int rows,int cols,int vert_offset_px,int horiz_offset_px) : base(G.Window,rows,cols,cell_h,cell_w,vert_offset_px,horiz_offset_px,filename,font_w,font_padding,aa_font){
			buttons = new PosArray<Button>(rows,cols);
			internal_main_button = GetFittedButton(0,0,rows,cols,Command.None);
		}
		public static ButtonPanel Create(int rows,int cols,int vert_offset_px,int horiz_offset_px,int corner = 0){
			int v = vert_offset_px;
			int h = horiz_offset_px;
			switch(corner){
			//case 0, do nothing
			case 1:
			h = G.Window.ClientRectangle.Width - cols*cell_w - horiz_offset_px;
			break;
			case 2:
			v = G.Window.ClientRectangle.Height - rows*cell_h - vert_offset_px;
			h = G.Window.ClientRectangle.Width - cols*cell_w - horiz_offset_px;
			break;
			case 3:
			v = G.Window.ClientRectangle.Height - rows*cell_h - vert_offset_px;
			break;
			}
			ButtonPanel p = new ButtonPanel(rows,cols,v,h);
			MouseUI.AddButton(p);
			return p;
		}
		public void Remove(){
			//panel.RemoveFromWindow(); todo broke all this
			MouseUI.RemoveButton(this);
			if(surface.window != null){
				surface.window.Surfaces.Remove(surface);
			}
			//MouseUI.RemoveButton(internal_main_button);
			/*if(internal_main_button != null){
				MouseUI.RemoveButton(internal_main_button);
			}
			else{
				foreach(Button b in buttonlist){
					MouseUI.RemoveButton(b);
				}
			}*/
		}
		//todo: ButtonAdd and ButtonRemove, for non-printing tasks?
		public void ButtonWrite(int row,int col,string s,Command command,int selection = -1){ //if there's already a button here, no new button will be created.
			ButtonWrite(row,col,s,colorchar.DefaultColor,colorchar.DefaultBackgroundColor,command,selection);
		}
		public void ButtonWrite(int row,int col,string s,Color4 color,Command command,int selection = -1){
			ButtonWrite(row,col,s,color,colorchar.DefaultBackgroundColor,command,selection);
		}
		public void ButtonWrite(int row,int col,string s,Color4 color,Color4 bgcolor,Command command,int selection = -1){
			Write(row,col,s,color,bgcolor);
			if(buttons[row,col] == null){
				int sel = selection;
				if(sel == -1){
					sel = row;
				}
				Button b = new Button(command,sel,surface.TotalYOffsetPx() + row*cell_h,surface.TotalXOffsetPx() + col*cell_w,cell_h,s.Length*cell_w);
				for(int j=col;j<col+s.Length;++j){
					buttons[row,j] = b;
				}
				internal_main_button.AddButton(b);
			}
		}
		public void NoButtonWrite(int row,int col,string s){ //no button is created. if a button is already here, it'll be removed.
			NoButtonWrite(row,col,s,colorchar.DefaultColor,colorchar.DefaultBackgroundColor);
		}
		public void NoButtonWrite(int row,int col,string s,Color4 color){
			NoButtonWrite(row,col,s,color,colorchar.DefaultBackgroundColor);
		}
		public void NoButtonWrite(int row,int col,string s,Color4 color,Color4 bgcolor){
			Write(row,col,s,color,bgcolor);
			if(buttons[row,col] != null){
				Button b = buttons[row,col];
				if(internal_main_button.sub_buttons != null){
					internal_main_button.sub_buttons.Remove(b);
				}
				for(int j=col;j<col+s.Length;++j){ //todo: how will this remove buttons from the buttonlist?
					buttons[row,j] = null;
				}
			}
		}
		public bool Clicked(Point p){ return internal_main_button.Clicked(p); }
		public DragTracker CheckDrag(Point p){ return internal_main_button.CheckDrag(p); }
		/*public Action<int,int> OnDrag(Point p){
			return internal_main_button.OnDrag(p);
		}*/
		public Rectangle BoundingRect(){ return internal_main_button.rect; }
		public void Move(int dx,int dy){
			//surface.ChangeOffsetInPixels(dx,dy);
			internal_main_button.Move(dx,dy);
		}
		public void MoveTo(int x,int y){
			//surface.SetOffsetInPixels(x,y);
			internal_main_button.MoveTo(x,y);
		}
		public ButtonImage GetFittedButton(int start_row,int start_col,int height_in_cells,int width_in_cells,Command command,int selection = -1){ //helper method to create a button based on cells.
			return new ButtonImage(command,selection,surface.TotalYOffsetPx() + start_row*cell_h,surface.TotalXOffsetPx() + start_col*cell_w,height_in_cells*cell_h,width_in_cells*cell_w,surface);
			//return new Button(command,selection,surface.TotalYOffsetPx() + start_row*cell_h,surface.TotalXOffsetPx() + start_col*cell_w,height_in_cells*cell_h,width_in_cells*cell_w);
		}
	}
}
