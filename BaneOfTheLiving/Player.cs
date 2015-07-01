//
using System;
using System.Collections.Generic;
using OpenTK.Graphics;
using GLDrawing;
using TextPanels;
using Utilities;
using Bane;
namespace Bane{
	public static class Player{
		public static Actor necro = null;
		public static List<Unit> minions = new List<Unit>();

		public static void BetweenBattleMenu(){
			MouseUI.PushMode(InputMode.ActionSelect);
			RaiseEnemyUnitMenu();
			MouseUI.PopMode();
			MouseUI.PushMode(InputMode.ActionSelect); //todo
			ReviveMinionMenu();
			MouseUI.PopMode();
		}
		public static void RaiseEnemyUnitMenu__OLD__(){
			const int cols = 60;
			const int cellh = 16;
			const int cellw = 8;
			List<Job> jobs = new List<Job>(); //rogue champion scout healer soldier wizard commoner, for now...
			foreach(Unit foe in G.enemy_team){ //todo: eventually this should check for defeated enemies
				jobs.Add(foe.job);
			}
			List<Species> undead = new List<Species>{Species.Zombie,Species.Skeleton,Species.Spirit,Species.Wight,Species.Mummy,Species.Shade};
			int list_length = Math.Max(jobs.Count,undead.Count);
			int max_job_skills = jobs.Greatest(x=>x.skills.Count);
			int max_undead_skills = undead.Greatest(x=>x.skills.Count);
			int bottom_row = 5 + Math.Max(10,list_length);
			int rows = 8 + Math.Max(10,list_length) + Math.Max(8,max_job_skills+max_undead_skills); //where 8 is the number of lines used for everything else.
			ButtonPanel menu = ButtonPanel.Create(rows,cols,(G.Window.ClientRectangle.Height - rows*cellh)/2,(G.Window.ClientRectangle.Width - cols*cellw)/2);
			//TextPanel menu = UI.CreatePanel(rows,cols,(G.Window.ClientRectangle.Height - rows*cellh)/2,(G.Window.ClientRectangle.Width - cols*cellw)/2);
			int current_job = 0;
			int current_undead = 0;
			bool cursor_on_undead = true;
			Unit u = null;
			Surface undead_sprite = Surface.Create(G.Window,"undead.png",2);
			undead_sprite.texture = G.undead.texture;
			CellLayout.CreateGrid(undead_sprite,1,1,108,88,175,465);
			undead_sprite.UpdateMethod = d => {
				d.positions = new List<int>{0};
				d.layouts = new List<int>{0};
				d.sprites = new List<int>{G.frame + ((G.Timer.Elapsed.Seconds/3)%4) * 4 + u.species.idx*16};
				d.sprite_types = new List<int>{0};
			};
			Surface hat_sprite = Surface.Create(G.Window,"living.png",2);
			hat_sprite.texture = G.living.texture;
			CellLayout.Create(hat_sprite,108,88,175,465,
				idx => {
					return undead_sprite.layouts[0].X(idx) + G.hats[u.species.idx,u.job.idx,0,G.frame].col;
				},
				idx => {
					return undead_sprite.layouts[0].Y(idx) + G.hats[u.species.idx,u.job.idx,0,G.frame].row; //todo: is the hat aligned correctly here? same size so it doesn't matter, right?
				});
			hat_sprite.UpdateMethod = d => {
				d.positions = new List<int>{0};
				d.layouts = new List<int>{0};
				d.sprites = new List<int>{u.job.idx * 8 + ((G.Timer.Elapsed.Seconds/3)%4)};
				d.sprite_types = new List<int>{1};
			};
			//colorchar.DefaultColor = Color4.Gainsboro;
			Color4 selection_color = Color4.White;
			Color4 highlight_color = Color4.Cyan;
			u = Unit.Create(0,undead[current_undead],jobs[current_job],null); //todo: should reflect living unit
			undead_sprite.Update();
			hat_sprite.Update();
			G.FourPerSecondUpdateMethods.Add(undead_sprite.Update);
			G.FourPerSecondUpdateMethods.Add(hat_sprite.Update);
			while(true){
				menu.Fill(TextPanel.GetBlackChar());
				menu.DrawBorder(new colorchar('-',Color4.LightGray),new colorchar(' ',Color4.LightGray),new colorchar('+',Color4.White));
				menu.Write(1,0,"Choose a vanquished foe to raise!".PadOuter(cols-2),Color4.WhiteSmoke);
				int count = 0;
				foreach(Species s in undead){
					Color4 color = Color4.DimGray;
					if(count == current_undead){
						if(cursor_on_undead){
							color = highlight_color;
						}
						else{
							color = selection_color;
						}
					}
					menu.Write(4 + count++,25,s.name,color);
				}
				count = 0;
				foreach(Job j in jobs){
					Color4 color = Color4.DimGray;
					if(count == current_job){
						if(cursor_on_undead){
							color = selection_color;
						}
						else{
							color = highlight_color;
						}
					}
					menu.Write(4 + count++,41,j.name,color);
				}
				menu.Write(bottom_row,2,"Health:");
				string half_heart = u.health % 2 == 0? "" : "'";
				menu.Write(bottom_row,14,"".PadRight(u.health / 2,'*') + half_heart,Color4.Red);
				menu.Write(bottom_row + 1,2,"Initiative:");
				string half_init = u.initiative % 2 == 0? "" : ".";
				menu.Write(bottom_row + 1,14,"".PadRight(u.initiative / 2,'!') + half_init,Color4.Lime);
				menu.Write(bottom_row + 2,2,"Movement:");
				menu.Write(bottom_row + 2,14,"".PadRight(u.movement,'>'),Color4.Cyan);
				menu.Write(bottom_row + 5,22,"Skills:");
				int i = 0;
				foreach(List<Skill> l in new List<Skill>[]{jobs[current_job].skills,undead[current_undead].skills}){
					foreach(Skill s in l){
						Color4 color = Color4.DimGray;
						if(u.skills.Contains(s)){
							color = Color4.White;
						}
						menu.Write(bottom_row + i++,35,s.Name(),color);
					}
				}
				i = 0;
				foreach(string s in new string[]{"Body","Equipment","Warfare","Support","Magic"}){
					Color4 color = Color4.White;
					if(undead[current_undead].blocked_skill_categories.Contains(i)){
						color = Color4.DimGray;
					}
					menu.Write(bottom_row + 7 + i++,22,s,color);
				}
				Command command = G.GetCommand();
				switch(command){
				case Command.Up:
				if(cursor_on_undead){
					current_undead = (current_undead - 1).Modulo(undead.Count);
				}
				else{
					current_job = (current_job - 1).Modulo(jobs.Count);
				}
				u = Unit.Create(0,undead[current_undead],jobs[current_job],null); //todo?
				break;
				case Command.Down:
				if(cursor_on_undead){
					current_undead = (current_undead + 1).Modulo(undead.Count);
				}
				else{
					current_job = (current_job + 1).Modulo(jobs.Count);
				}
				u = Unit.Create(0,undead[current_undead],jobs[current_job],null); //todo?
				break;
				case Command.Left:
				if(!cursor_on_undead){
					cursor_on_undead = true;
				}
				break;
				case Command.Right:
				if(cursor_on_undead){
					cursor_on_undead = false;
				}
				break;
				case Command.Enter:
				minions.Add(u);
				menu.Remove();
				G.Window.Surfaces.Remove(undead_sprite); //todo
				G.Window.Surfaces.Remove(hat_sprite);
				return;
				}
			}
		}
		public static void RaiseEnemyUnitMenu(){
			const int cols = 60;
			const int cellh = 16;
			const int cellw = 8;
			List<Job> jobs = new List<Job>(); //rogue champion scout healer soldier wizard commoner, for now...
			foreach(Unit foe in G.enemy_team){ //todo: eventually this should check for defeated enemies
				jobs.Add(foe.job);
			}
			List<Species> undead = new List<Species>{Species.Zombie,Species.Skeleton,Species.Spirit,Species.Wight,Species.Mummy,Species.Shade};
			int list_length = Math.Max(jobs.Count,undead.Count);
			int max_job_skills = jobs.Greatest(x=>x.skills.Count);
			int max_undead_skills = undead.Greatest(x=>x.skills.Count);
			int bottom_row = 5 + Math.Max(10,list_length);
			int rows = 8 + Math.Max(10,list_length) + Math.Max(8,max_job_skills+max_undead_skills); //where 8 is the number of lines used for everything else.
			ButtonPanel menu = ButtonPanel.Create(rows,cols,(G.Window.ClientRectangle.Height - rows*cellh)/2,(G.Window.ClientRectangle.Width - cols*cellw)/2);
			int current_job = 0;
			int current_undead = 0;
			bool cursor_on_undead = true;
			Unit u = Unit.Create(0,undead[current_undead],jobs[current_job],null); //todo: should reflect living unit //todo: also check team here
			List<Surface> sprite = UI.CreateSingleSprite(u,175,465);
			//colorchar.DefaultColor = Color4.Gainsboro;
			Color4 selection_color = Color4.White;
			Color4 highlight_color = Color4.Cyan;
			while(true){
				menu.Fill(TextPanel.GetBlackChar());
				menu.DrawBorder(new colorchar('-',Color4.LightGray),new colorchar(' ',Color4.LightGray),new colorchar('+',Color4.White));
				menu.Write(1,0,"Choose a vanquished foe to raise!".PadOuter(cols-2),Color4.WhiteSmoke);
				int count = 0;
				foreach(Species s in undead){
					Color4 color = Color4.DimGray;
					if(count == current_undead){
						if(cursor_on_undead){
							color = highlight_color;
						}
						else{
							color = selection_color;
						}
					}
					menu.Write(4 + count++,25,s.name,color);
				}
				count = 0;
				foreach(Job j in jobs){
					Color4 color = Color4.DimGray;
					if(count == current_job){
						if(cursor_on_undead){
							color = selection_color;
						}
						else{
							color = highlight_color;
						}
					}
					menu.Write(4 + count++,41,j.name,color);
				}
				menu.Write(bottom_row,2,"Health:");
				string half_heart = u.health % 2 == 0? "" : "'"; //todo: put this in a method somewhere, "get health string"
				menu.Write(bottom_row,14,"".PadRight(u.health / 2,'*') + half_heart,Color4.Red);
				menu.Write(bottom_row + 1,2,"Initiative:");
				string half_init = u.initiative % 2 == 0? "" : ".";
				menu.Write(bottom_row + 1,14,"".PadRight(u.initiative / 2,'!') + half_init,Color4.Lime);
				menu.Write(bottom_row + 2,2,"Movement:");
				menu.Write(bottom_row + 2,14,"".PadRight(u.movement,'>'),Color4.Cyan);
				menu.Write(bottom_row + 5,22,"Skills:");
				int i = 0;
				foreach(List<Skill> l in new List<Skill>[]{jobs[current_job].skills,undead[current_undead].skills}){
					foreach(Skill s in l){
						Color4 color = Color4.DimGray;
						if(u.skills.Contains(s)){
							color = Color4.White;
						}
						menu.Write(bottom_row + i++,35,s.Name(),color);
					}
				}
				i = 0;
				foreach(string s in new string[]{"Body","Equipment","Warfare","Support","Magic"}){
					Color4 color = Color4.White;
					if(undead[current_undead].blocked_skill_categories.Contains(i)){
						color = Color4.DimGray;
					}
					menu.Write(bottom_row + 7 + i++,22,s,color);
				}
				Command command = G.GetCommand();
				switch(command){
				case Command.Up:
				if(cursor_on_undead){
					current_undead = (current_undead - 1).Modulo(undead.Count);
				}
				else{
					current_job = (current_job - 1).Modulo(jobs.Count);
				}
				u.Change(undead[current_undead],jobs[current_job]);
				UI.UpdateSingleSprite(sprite);
				break;
				case Command.Down:
				if(cursor_on_undead){
					current_undead = (current_undead + 1).Modulo(undead.Count);
				}
				else{
					current_job = (current_job + 1).Modulo(jobs.Count);
				}
				u.Change(undead[current_undead],jobs[current_job]);
				UI.UpdateSingleSprite(sprite);
				break;
				case Command.Left:
				if(!cursor_on_undead){
					cursor_on_undead = true;
				}
				break;
				case Command.Right:
				if(cursor_on_undead){
					cursor_on_undead = false;
				}
				break;
				case Command.Enter:
				minions.Add(u); //todo: again, verify team number.
				menu.Remove();
				UI.RemoveSingleSprite(sprite);
				return;
				}
			}
		}
		public static void ReviveMinionMenu(){
			const int rows = 30;
			const int cols = 60;
			const int cellh = 16;
			const int cellw = 8;
			List<Unit> fallen = new List<Unit>();
			foreach(Unit u in minions){
				if(u.health > 0){ //todo: check for defeated status here
					continue;
				}
				fallen.Add(u);
			}
			if(fallen.Count == 0){
				return;
			}
			ButtonPanel menu = ButtonPanel.Create(rows,cols,(G.Window.ClientRectangle.Height - rows*cellh)/2,(G.Window.ClientRectangle.Width - cols*cellw)/2);
			int current = 0;
			while(true){
				menu.Fill(TextPanel.GetBlackChar());
				menu.DrawBorder(new colorchar('-',Color4.LightGray),new colorchar(' ',Color4.LightGray),new colorchar('+',Color4.White));
				menu.Write(1,0,"Revive a fallen minion!".PadOuter(cols-2),Color4.WhiteSmoke);
				int count = 0;
				foreach(Unit u in minions){
					Color4 color = Color4.DimGray;
					if(fallen.Contains(u)){
						if(fallen[current] == u){
							color = Color4.Cyan;
						}
						else{
							color = Color4.White;
						}
						menu.Write(3 + count*2,0,u.name.PadRight(24) + "(FALLEN)",color);
					}
					else{
						menu.Write(3 + count*2,0,u.name,color);
					}
					if(count++ >= 8){
						break;
					}
				}
				Command command = G.GetCommand();
				switch(command){
				case Command.Up:
				current = (current - 1).Modulo(fallen.Count);
				break;
				case Command.Down:
				current = (current + 1).Modulo(fallen.Count);
				break;
				case Command.Enter:
				//todo: restore defeated unit
				//fallen[current].health = fallen[current].absolute_max_health;
				menu.Remove();
				return;
				}
			}
		}
	}
}
