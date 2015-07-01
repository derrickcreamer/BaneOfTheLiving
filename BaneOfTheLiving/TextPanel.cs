//
using System;
using OpenTK.Graphics;
using GLDrawing;
using PosArrays;
namespace TextPanels{
	public struct colorchar{
		public char c;
		public Color4 color;
		public Color4 bgcolor;

		public static Color4 DefaultColor = Color4.White;
		public static Color4 DefaultBackgroundColor = Color4.Black;

		public colorchar(char c_,Color4 color_,Color4 bgcolor_){
			c = c_;
			color = color_;
			bgcolor = bgcolor_;
		}
		public colorchar(char c_,Color4 color_){
			c = c_;
			color = color_;
			bgcolor = DefaultBackgroundColor;
		}
		public colorchar(char c_){
			c = c_;
			color = DefaultColor;
			bgcolor = DefaultBackgroundColor;
		}
	}
	public class TextPanel{
		public Surface surface;
		protected PosArray<colorchar> memory;
		public bool NoUpdate = false; //if NoUpdate is true, changing 'memory' will not update the corresponding data in 'surface'. This is useful if you wish to update lots at once, instead of one at a time.
		protected int height;
		protected int width;
		public int HeightPx{ get{ return height; } }
		public int WidthPx{ get{ return width; } }
		public int Rows{ get{ return memory.objs.GetLength(0); } }
		public int Cols{ get{ return memory.objs.GetLength(1); } }
		public colorchar this[int row,int col]{
			get{
				return memory[row,col];
			}
			set{
				if(!memory[row,col].Equals(value)){
					memory[row,col] = value;
					if(!NoUpdate){
						UpdateSurface(col + row * memory.objs.GetLength(1)); //todo: fix all this 1D indexing, for speed.
					}
				}
			}
		}
		public colorchar this[pos p]{
			get{
				return memory[p];
			}
			set{
				if(!memory[p].Equals(value)){
					memory[p] = value;
					if(!NoUpdate){
						UpdateSurface(p.col + p.row * memory.objs.GetLength(1)); //todo: can UpdateSurface get a 2d version?
					}
				}
			}
		}
		public colorchar this[int idx]{
			get{
				return memory[idx];
			}
			set{
				if(!memory[idx].Equals(value)){
					memory[idx] = value;
					if(!NoUpdate){
						UpdateSurface(idx);
					}
				}
			}
		}
		public TextPanel(GLWindow parent_window,int rows,int cols,int cell_h_px,int cell_w_px,int v_offset_px,int h_offset_px,string font_filename,int char_width_px,int padding_between_chars_px,bool antialiased_font){
			memory = new PosArray<colorchar>(rows,cols);
			colorchar cch = TextPanel.GetBlackChar();
			for(int i=0;i<rows;++i){
				for(int j=0;j<cols;++j){
					memory[i,j] = cch;
				}
			}
			height = rows * cell_h_px;
			width = cols * cell_w_px;
			string shader = antialiased_font? Shader.AAFontFS() : Shader.FontFS();
			surface = Surface.Create(parent_window,font_filename,shader,false,2,4,4); //todo: maybe a bool to control whether the panel gets added to the window's list?
			SpriteType.DefineSingleRowSprite(surface,char_width_px,padding_between_chars_px); //also todo, TransparentFontFS exists now...?
			CellLayout.CreateGrid(surface,rows,cols,cell_h_px,cell_w_px,0,0);
			surface.SetOffsetInPixels(h_offset_px,v_offset_px);
			surface.SetEasyLayoutCounts(rows*cols);
			surface.DefaultUpdatePositions();
			UpdateSurface(0,rows*cols-1);
		}
		/*public static TextPanel Create(GLWindow parent_window,int rows,int cols,int cell_h_px,int cell_w_px,int v_offset_px,int h_offset_px,string font_filename,int char_width_px,int padding_between_chars_px,bool antialiased_font){
			TextPanel p = new TextPanel();
			p.memory = new PosArray<colorchar>(rows,cols);
			colorchar cch = TextPanel.GetBlackChar();
			for(int i=0;i<rows;++i){
				for(int j=0;j<cols;++j){
					p.memory[i,j] = cch;
				}
			}
			p.height = rows * cell_h_px;
			p.width = cols * cell_w_px;
			string shader = antialiased_font? Shader.AAFontFS() : Shader.FontFS();
			p.surface = Surface.Create(parent_window,font_filename,shader,false,2,4,4); //todo: maybe a bool to control whether the panel gets added to the window's list?
			SpriteType.DefineSingleRowSprite(p.surface,char_width_px,padding_between_chars_px); //also todo, TransparentFontFS exists now...?
			CellLayout.CreateGrid(p.surface,rows,cols,cell_h_px,cell_w_px,0,0);
			p.surface.SetOffsetInPixels(h_offset_px,v_offset_px);
			p.surface.SetEasyLayoutCounts(rows*cols);
			p.surface.DefaultUpdatePositions();
			p.UpdateSurface(0,rows*cols-1);
			return p;
		}*/
		public void RemoveFromWindow(){
			if(surface.window != null){
				surface.window.Surfaces.Remove(surface);
			}
		}
		public static colorchar GetTransparentChar(){ return new colorchar(' ',Color4.Transparent,Color4.Transparent); }
		public static colorchar GetBlackChar(){ return new colorchar(' ',Color4.Black,Color4.Black); }
		public PosArray<colorchar> GetCurrentScreen(){
			int rows = memory.objs.GetLength(0);
			int cols = memory.objs.GetLength(1);
			PosArray<colorchar> result = new PosArray<colorchar>(rows,cols);
			for(int i=0;i<rows;++i){
				for(int j=0;j<cols;++j){
					result[i,j] = memory[i,j];
				}
			}
			return result;
		}
		public PosArray<colorchar> GetCurrentRect(int row,int col,int height,int width){
			PosArray<colorchar> result = new PosArray<colorchar>(height,width);
			for(int i=0;i<height;++i){
				for(int j=0;j<width;++j){
					result[i,j] = memory[row+i,col+j];
				}
			}
			return result;
		}
		public void Write(int row,int col,char ch){ //todo: every combination goes here. 3 indexers * 3-4 colorchar parameter configurations, per data type.
			this[row,col] = new colorchar(ch);
		}
		public void Write(int row,int col,char ch,Color4 color){
			this[row,col] = new colorchar(ch,color);
		}
		public void Write(int row,int col,char ch,Color4 color,Color4 bgcolor){
			this[row,col] = new colorchar(ch,color,bgcolor);
		}
		public void Write(int row,int col,colorchar ch){
			this[row,col] = ch;
		}
		public void Write(int row,int col,string s){
			Write(row,col,s,colorchar.DefaultColor,colorchar.DefaultBackgroundColor);
		}
		public void Write(int row,int col,string s,Color4 color){
			Write(row,col,s,color,colorchar.DefaultBackgroundColor);
		}
		public void Write(int row,int col,string s,Color4 color,Color4 bgcolor){
			int c = col;
			bool update = false; //todo: bounds check?
			int start_col = -1;
			int end_col = -1;
			foreach(char ch in s){
				if(c >= memory.objs.GetLength(1)){
					break;
				}
				colorchar cch = new colorchar(ch,color,bgcolor);
				if(!memory[row,c].Equals(cch)){
					memory[row,c] = cch;
					update = true;
					if(start_col == -1){
						start_col = c;
					}
					end_col = c;
				}
				++c;
			}
			if(update && !NoUpdate){
				UpdateSurface(start_col + row * memory.objs.GetLength(1),end_col + row * memory.objs.GetLength(1)); //todo: fix all this 1D indexing, for speed.
			}
		}
		public void Clear(Color4 clear_color){
			int rows = memory.objs.GetLength(0);
			int cols = memory.objs.GetLength(1);
			colorchar cch = new colorchar(' ',clear_color,clear_color);
			for(int i=0;i<rows;++i){
				for(int j=0;j<cols;++j){
					memory[i,j] = cch;
				}
			}
			UpdateSurface(0,rows*cols - 1);
		}
		public void Fill(colorchar fill_char){
			int rows = memory.objs.GetLength(0);
			int cols = memory.objs.GetLength(1);
			for(int i=0;i<rows;++i){
				for(int j=0;j<cols;++j){
					memory[i,j] = fill_char;
				}
			}
			UpdateSurface(0,rows*cols - 1);
		}
		public void DrawBorder(colorchar border_char){
			DrawBorder(border_char,border_char,border_char,border_char,border_char,border_char,border_char,border_char);
		}
		public void DrawBorder(colorchar edge_char,colorchar corner_char){
			DrawBorder(edge_char,edge_char,edge_char,edge_char,corner_char,corner_char,corner_char,corner_char);
		}
		public void DrawBorder(colorchar top_bottom_char,colorchar right_left_char,colorchar corner_char){
			DrawBorder(top_bottom_char,top_bottom_char,right_left_char,right_left_char,corner_char,corner_char,corner_char,corner_char);
		}
		public void DrawBorder(colorchar n,colorchar s,colorchar e,colorchar w,colorchar ne,colorchar nw,colorchar se,colorchar sw){
			int rows = memory.objs.GetLength(0);
			int cols = memory.objs.GetLength(1);
			memory[0,0] = nw;
			memory[0,cols-1] = ne;
			memory[rows-1,0] = sw;
			memory[rows-1,cols-1] = se;
			for(int j=1;j<cols-1;++j){
				memory[0,j] = n;
				memory[rows-1,j] = s;
			}
			for(int i=1;i<rows-1;++i){
				memory[i,0] = w;
				memory[i,cols-1] = e;
			}
			UpdateSurface(0,rows*cols - 1);
		}
		public void UpdateSurface(int start_idx,int end_idx){
			int count = (end_idx - start_idx) + 1;
			int[] sprite_idx = new int[count];
			float[][] color = new float[2][];
			color[0] = new float[4 * count];
			color[1] = new float[4 * count];
			for(int i=0;i<count;++i){
				colorchar cch = memory[start_idx + i];
				sprite_idx[i] = (int)(cch.c);
				int i4 = i*4;
				color[0][i4] = cch.color.R;
				color[0][i4+1] = cch.color.G;
				color[0][i4+2] = cch.color.B;
				color[0][i4+3] = cch.color.A;
				color[1][i4] = cch.bgcolor.R;
				color[1][i4+1] = cch.bgcolor.G;
				color[1][i4+2] = cch.bgcolor.B;
				color[1][i4+3] = cch.bgcolor.A;
			}
			surface.window.UpdateOtherVertexArray(surface,start_idx,sprite_idx,new int[count],color);
		}
		public void UpdateSurface(int idx){
			float[][] color = new float[2][];
			color[0] = new float[4];
			color[1] = new float[4];
			int cols = memory.objs.GetLength(1);
			colorchar cch = memory[idx / cols,idx % cols];
			int sprite_idx = (int)(cch.c);
			color[0][0] = cch.color.R;
			color[0][1] = cch.color.G;
			color[0][2] = cch.color.B;
			color[0][3] = cch.color.A;
			color[1][0] = cch.bgcolor.R;
			color[1][1] = cch.bgcolor.G;
			color[1][2] = cch.bgcolor.B;
			color[1][3] = cch.bgcolor.A;
			surface.window.UpdateOtherSingleVertex(surface,idx,sprite_idx,0,color);
		}
		//
		/*
		 * methods?
bounds check, or is that part of something else?
update with given array? not sure. is this used for highlights?
write char
write array
write list
write string
(static?) color resolution stuff?

*/
	}
}
