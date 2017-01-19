using System.Collections.Generic;
using System.Text;
using Font = SharpDX.Direct3D9.Font;
using SharpDX.Direct3D9;
using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = System.Drawing.Color;
namespace Movement_predict
{
    class Program
    {
        private static Menu MovpredMenu, settingsMenu;
        private static AIHeroClient Player { get { return ObjectManager.Player; } }
        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }
        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Drawing.OnDraw += Drawing_OnDraw;
            MovpredMenu = MainMenu.AddMenu("Movement Drawer", "Movement Drawer");
            settingsMenu = MovpredMenu.AddSubMenu("Settings");
            settingsMenu.AddGroupLabel("Statut");
            settingsMenu.Add("On", new CheckBox("Enabled"));
            settingsMenu.AddGroupLabel("Draw movements from");
            foreach (var e in EntityManager.Heroes.Enemies)
            {
                settingsMenu.Add(e.ChampionName.ToString(), new CheckBox(e.ChampionName));
            }
            settingsMenu.Add("range", new Slider("Draw enemies movement in a range of", 1500, 1, 3000));
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            int avoidriding = 0;
            foreach (var e in EntityManager.Heroes.Enemies.Where(a => !a.IsDead && a.IsHPBarRendered))
            {
                if (Player.Distance(e) < settingsMenu["range"].Cast<Slider>().CurrentValue && settingsMenu["on"].Cast<CheckBox>().CurrentValue && settingsMenu[e.ChampionName.ToString()].Cast<CheckBox>().CurrentValue)
                {
                   
                    var enemypath = Prediction.Position.GetRealPath(e);
                    var enemypathxyz = enemypath[1];
                    // Chat.Print("Enemypath : " + enemypath[0]);
                    // var test = e.Position;
                    var color = 250 > Player.Distance(enemypathxyz) ? System.Drawing.Color.Red : System.Drawing.Color.GreenYellow;
                    new Circle() { Color = color, BorderWidth = 4, Radius = 60 }.Draw(enemypath[1]);
                    var line = new Geometry.Polygon.Line(e.Position, enemypathxyz, e.Distance(enemypathxyz)); line.Draw(color, 2);
                    // Chat.Print(e.HPBarPosition.Y);
                    if (250 > Player.Distance(enemypathxyz))
                    {
                        avoidriding += 20; // Avoid DrawText riding
                        EloBuddy.Drawing.DrawText((int)Player.HPBarPosition.X + 20, (int)Player.HPBarPosition.Y + 110 + avoidriding, Color.Red, "Care " + e.ChampionName, 6);
                    }
                }
            }
        }
    }
}
