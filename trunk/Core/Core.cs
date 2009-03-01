using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
//using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
//using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.Collections;
using MiniGameInterfaces;
using System.IO;
//using System.Windows.Forms;
//using System.Drawing;
namespace CoreNamespace
{
    
    public class Core : IGame
    {
        /// <summary>
        /// map border. unit is out of borders if abs(x)>border||abs(y)>border
        /// </summary>
        public const int Border = 4000;
        internal static GameObjectsClass gameObjects;
        List<IAI> players;
        string[] playersText;
        float[] playersTotalUpdateTime;
        float coreTotalUpdateTime;
        bool gameEnd;
        bool gameDraw;
        int gameWinner;
        int[] destroyers;
        int[] corvettes;
        int[] cruisers;
        int[] total;
        bool gameEndSoon;
        float endOfGameTimer;
        public Core(int ScreenWidth, int ScreenHeight, ContentManager content, Microsoft.Xna.Framework.GraphicsDeviceManager graphics)
        {
            gameObjects = new GameObjectsClass();
            timing = new TimingClass();
            viewer = new Viewer(ScreenWidth, ScreenHeight, content, graphics);
            units = new List<Unit>();
            shots = new Shots(units);
        }
        static TimingClass timing;
        public static TimingClass Timing
        {
            get { return timing; }
        }
        static internal Shots shots;
        public static Viewer viewer;
        internal static int CurrentPlayer;
        public string SecondsToString(float seconds)
        {
            int iMinutes = (int)(seconds / 60);
            seconds -= iMinutes * 60;
            int iSeconds = (int)seconds;
            seconds -= iSeconds;
            int iMillis = (int)(seconds * 1000);
            string sSeconds = iSeconds.ToString();
            while (sSeconds.Length < 2)
                sSeconds = "0" + sSeconds;
            string sMillis = iMillis.ToString();
            while (sMillis.Length < 4)
                sMillis = "0" + sMillis;
            return iMinutes.ToString() + ":" + sSeconds + "." + sMillis;
        }
        public void CalculateNumberOfUnits()
        {
            for (int i = 0; i < players.Count; i++)
            {
                destroyers[i] = 0;
                corvettes[i] = 0;
                cruisers[i] = 0;
                total[i] = 0;
            }
            for (int i = 0; i < units.Count; i++)
            {
                switch (units[i].ShipType)
                {
                    case ShipTypes.Destroyer:
                        destroyers[units[i].PlayerOwner]++;
                        break;
                    case ShipTypes.Corvette:
                        corvettes[units[i].PlayerOwner]++;
                        break;
                    case ShipTypes.Cruiser:
                        cruisers[units[i].PlayerOwner]++;
                        break;
                }
                total[units[i].PlayerOwner]++;
            }
        }
        public void CheckEndOfGame()
        {
            if (gameEnd)
                return;
            if (!gameEndSoon)
            {
                gameEndSoon = true;
                gameWinner = -1;
                for (int i = 0; i < players.Count; i++)
                {
                    if (total[i] > 0)
                    {
                        if (gameWinner != -1)
                        {
                            gameEndSoon = false;
                            break;
                        }
                        gameWinner = i;
                    }
                }
                if (gameEndSoon)
                    endOfGameTimer = 2.0f; //Maybe this could be more precise
            }
            else
            {
                endOfGameTimer -= Timing.DeltaTime;
                if (endOfGameTimer < 0)
                {
                    gameEnd = true;
                    gameDraw = (units.Count == 0);
                    gameWinner = -1;
                    for (int i = 0; i < players.Count; i++)
                    {
                        if (total[i] > 0)
                        {
                            if (gameWinner != -1)
                            {
                                gameEnd = false;
                                break;
                            }
                            gameWinner = i;
                        }
                    }
                }
            }
        }
        public void Draw()
        {
            Core.viewer.graphics.GraphicsDevice.Clear(Microsoft.Xna.Framework.Graphics.Color.Black);
            viewer.Update();

            //
            viewer.DrawEnvironment();
            viewer.DrawUnits(units);
            viewer.DrawShots(shots);
            //
            string[] infoString = new string[players.Count];
            string[] timeString = new string[players.Count];
            for (int i = 0; i < players.Count; i++)
            {
                infoString[i] = destroyers[i].ToString() + "+" + corvettes[i].ToString() + "+" + cruisers[i].ToString() + "=" + total[i].ToString();
                timeString[i] = SecondsToString(playersTotalUpdateTime[i]);
            }
            string coreTimeString = SecondsToString(coreTotalUpdateTime);
            //
            string[] lines;
            viewer.DrawText(players[0].Author, new GameVector(100, 20), 0, new Microsoft.Xna.Framework.Graphics.Color(Viewer.TeamColors[0]));
            viewer.DrawText(players[0].Description, new GameVector(100, 40), 0, Microsoft.Xna.Framework.Graphics.Color.Gray);
            viewer.DrawText(infoString[0], new GameVector(100, 60), 0, Microsoft.Xna.Framework.Graphics.Color.White);
            viewer.DrawText(timeString[0], new GameVector(100, 80), 0, Microsoft.Xna.Framework.Graphics.Color.White);
            lines = playersText[0].Split(new char[] { '\n' });
            for (int i = 0; i < lines.Length; i++)
                viewer.DrawText(lines[i], new GameVector(100, 120 + i * 20), 0, Microsoft.Xna.Framework.Graphics.Color.Yellow);
            viewer.DrawText("vs.", new GameVector(Core.viewer.screenWidth / 2, 20), 1, Microsoft.Xna.Framework.Graphics.Color.White);
            viewer.DrawText("Core update time:", new GameVector(Core.viewer.screenWidth / 2, 60), 1, Microsoft.Xna.Framework.Graphics.Color.White);
            viewer.DrawText(coreTimeString, new GameVector(Core.viewer.screenWidth / 2, 80), 1, Microsoft.Xna.Framework.Graphics.Color.White);
            viewer.DrawText(players[1].Author, new GameVector(Core.viewer.screenWidth - 100, 20), 2, new Microsoft.Xna.Framework.Graphics.Color(Viewer.TeamColors[1]));
            viewer.DrawText(players[1].Description, new GameVector(Core.viewer.screenWidth - 100, 40), 2, Microsoft.Xna.Framework.Graphics.Color.Gray);
            viewer.DrawText(infoString[1], new GameVector(Core.viewer.screenWidth - 100, 60), 2, Microsoft.Xna.Framework.Graphics.Color.White);
            viewer.DrawText(timeString[1], new GameVector(Core.viewer.screenWidth - 100, 80), 2, Microsoft.Xna.Framework.Graphics.Color.White);
            lines = playersText[1].Split(new char[] { '\n' });
            for (int i = 0; i < lines.Length; i++)
                viewer.DrawText(lines[i], new GameVector(Core.viewer.screenWidth - 100, 120 + i * 20), 2, Microsoft.Xna.Framework.Graphics.Color.Yellow);
            //
            if (gameEnd)
            {
                if (gameDraw)
                {
                    viewer.DrawText("DRAW!", new GameVector(Core.viewer.screenWidth / 2, Core.viewer.screenHeight / 2), 1, Microsoft.Xna.Framework.Graphics.Color.White);
                }
                else
                {
                    viewer.DrawText("Winner:", new GameVector(Core.viewer.screenWidth / 2, Core.viewer.screenHeight / 2), 1, Microsoft.Xna.Framework.Graphics.Color.White);
                    viewer.DrawText(players[gameWinner].Author, new GameVector(Core.viewer.screenWidth / 2, Core.viewer.screenHeight / 2 + 20), 1, new Microsoft.Xna.Framework.Graphics.Color(Viewer.TeamColors[gameWinner]));
                    viewer.DrawText(players[gameWinner].Description, new GameVector(Core.viewer.screenWidth / 2, Core.viewer.screenHeight / 2 + 40), 1, Microsoft.Xna.Framework.Graphics.Color.Gray);
                }
            }
            //
            viewer.DrawText("Time speed: " + Timing.TimeSpeed.ToString(), new GameVector(10, Core.viewer.screenHeight - 30), 0, Microsoft.Xna.Framework.Graphics.Color.White);
            ///////////DEBUG DRAW DEMO
            //viewer.DrawRectangle(new MiniGameInterfaces.Rectangle(GameVector.Zero, GameVector.One*100, GameVector.UnitX), new MiniGameInterfaces.Color(0, 1, 0, 1.0f));
            //viewer.DrawRectangle(new MiniGameInterfaces.Rectangle(GameVector.One*50, GameVector.One * 100, GameVector.UnitX.Rotate(Timing.NowTime)), new MiniGameInterfaces.Color(1f, 0, 0, 0.5f));
            //viewer.DrawCircle(new Circle(GameVector.One*(-100),70.7f), MiniGameInterfaces.Color.Blue);
            //viewer.DrawPoint(new GameVector(-50, 50), new MiniGameInterfaces.Color(1, 0, 0, 1));
            //viewer.DrawLine( new Stretch(GameVector.Zero,GameVector.UnitX.Rotate(-timing.NowTime)*3*70.7f),MiniGameInterfaces.Color.Blue);
            /////////////COLLIZION TEST 1
            //Rectangle rect1 = new Rectangle(GameVector.Zero, GameVector.One * 200, GameVector.UnitX);
            //Stretch stretch1 = new Stretch(new GameVector(0, 150), new GameVector(50, 50));
            //Color col;
            //if (rect1.IntersectsLine(stretch1.pt1,stretch1.pt2))
            //    col=Color.Red;
            //else col=Color.Green;
            //viewer.DrawRectangle(rect1, col);
            //viewer.DrawLine(stretch1, col);
            /////////////COLLIZION TEST 2
            //Rectangle rect1 = new Rectangle(GameVector.Zero, GameVector.One * 200, GameVector.UnitX);
            //Stretch stretch1 = new Stretch(new GameVector(50, 100), new GameVector(150, 150));
            //Color col;
            //if (rect1.IntersectsLine(stretch1.pt1, stretch1.pt2))
            //    col = Color.Red;
            //else col = Color.Green;
            //viewer.DrawRectangle(rect1, col);
            //viewer.DrawLine(stretch1, col);
            /////////////COLLIZION TEST 3
            //Rectangle rect1 = new Rectangle(GameVector.Zero, GameVector.One * 200, GameVector.UnitX);
            //Stretch stretch1 = new Stretch(new GameVector(50, 150), new GameVector(150, 50));
            //Color col;
            //if (rect1.IntersectsLine(stretch1.pt1, stretch1.pt2))
            //    col = Color.Red;
            //else col = Color.Green;
            //viewer.DrawRectangle(rect1, col);
            //viewer.DrawLine(stretch1, col);
            /////////////COLLIZION TEST 4
            //Rectangle rect1 = new Rectangle(GameVector.Zero, GameVector.One * 200, GameVector.UnitX);
            //Stretch stretch1 = new Stretch(new GameVector(100, 100), new GameVector(150, 150));
            //Color col;
            //if (rect1.IntersectsLine(stretch1.pt1, stretch1.pt2))
            //    col = Color.Red;
            //else col = Color.Green;
            //viewer.DrawRectangle(rect1, col);
            //viewer.DrawLine(stretch1, col);
            /////////////COLLIZION TEST 5
            //Rectangle rect1 = new Rectangle(GameVector.Zero, GameVector.One * 200, GameVector.UnitX);
            //Stretch stretch1 = new Stretch(new GameVector(-50, 100), new GameVector(50, 100));
            //Color col;
            //if (rect1.IntersectsLine(stretch1.pt1, stretch1.pt2))
            //    col = Color.Red;
            //else col = Color.Green;
            //viewer.DrawRectangle(rect1, col);
            //viewer.DrawLine(stretch1, col);
            /////////////COLLIZION TEST 6
            //Rectangle rect1 = new Rectangle(GameVector.Zero, GameVector.One * 200, GameVector.UnitX);
            //Stretch stretch1 = new Stretch(new GameVector(-50, 100+1), new GameVector(50, 100+1));
            //Color col;
            //if (rect1.IntersectsLine(stretch1.pt1, stretch1.pt2))
            //    col = Color.Red;
            //else col = Color.Green;
            //viewer.DrawRectangle(rect1, col);
            //viewer.DrawLine(stretch1, col);
            /////////////COLLIZION TEST 7
            //Rectangle rect1 = new Rectangle(GameVector.Zero, GameVector.One * 200, GameVector.UnitX);
            //Stretch stretch1 = new Stretch(new GameVector(50, 100), new GameVector(150, 100));
            //Color col;
            //if (rect1.IntersectsLine(stretch1.pt1, stretch1.pt2))
            //    col = Color.Red;
            //else col = Color.Green;
            //viewer.DrawRectangle(rect1, col);
            //viewer.DrawLine(stretch1, col);
            /////////////COLLIZION TEST 8
            //Rectangle rect1 = new Rectangle(GameVector.Zero, GameVector.One * 200, GameVector.UnitX);
            //Stretch stretch1 = new Stretch(new GameVector(150, 100), new GameVector(200, 100));
            //Color col;
            //if (rect1.IntersectsLine(stretch1.pt1, stretch1.pt2))
            //    col = Color.Red;
            //else col = Color.Green;
            //viewer.DrawRectangle(rect1, col);
            //viewer.DrawLine(stretch1, col);
            /////////////COLLIZION TEST 9
            //Circle rect1 = new Circle(GameVector.Zero, 100);
            //Stretch stretch1 = new Stretch(new GameVector(0, 75), new GameVector(150, 75));
            //Color col;
            //if ( rect1.Intersects(stretch1))
            //    col = Color.Red;
            //else col = Color.Green;
            //viewer.DrawCircle(rect1, col);
            //viewer.DrawLine(stretch1, col);
            /////////////COLLIZION TEST 10
            //Circle rect1 = new Circle(GameVector.Zero, 100);
            //Stretch stretch1 = new Stretch(new GameVector(-150, 75), new GameVector(150, 75));
            //Color col;
            //if (rect1.Intersects(stretch1))
            //    col = Color.Red;
            //else col = Color.Green;
            //viewer.DrawCircle(rect1, col);
            //viewer.DrawLine(stretch1, col);
            /////////////COLLIZION TEST 11
            //Circle rect1 = new Circle(GameVector.Zero, 100);
            //Stretch stretch1 = new Stretch(new GameVector(-150, 100), new GameVector(150, 100));
            //Color col;
            //if (rect1.Intersects(stretch1))
            //    col = Color.Red;
            //else col = Color.Green;
            //viewer.DrawCircle(rect1, col);
            //viewer.DrawLine(stretch1, col);
            ///////////COLLIZION TEST 12
            //Circle rect1 = new Circle(GameVector.Zero, 100);
            //Stretch stretch1 = new Stretch(new GameVector(100, 0), new GameVector(200, 0));
            //Color col;
            //if (rect1.Intersects(stretch1))
            //    col = Color.Red;
            //else col = Color.Green;
            //viewer.DrawCircle(rect1, col);
            //viewer.DrawLine(stretch1, col);
        }

        public void Update()
        {
            if (gameEnd)
                return;
            //AI update
            Stopwatch sw = new Stopwatch();
            for (CurrentPlayer = 0; CurrentPlayer < players.Count; CurrentPlayer++)
            {
                sw.Reset();
                sw.Start();
                players[CurrentPlayer].Update();
                playersTotalUpdateTime[CurrentPlayer] += ((float)sw.ElapsedTicks) / Stopwatch.Frequency;
            }
            //Core update
            CurrentPlayer = -1;
            sw.Reset();
            sw.Start();
            UnitIntersections();
            shots.Update();
            for (int i = 0; i < units.Count; i++)
            {
                units[i].Update();
                //units[i].SetAngle(205);
                if (units[i].IsDying)
                    DamageAllAround(units[i].position, units[i].BlowRadius, units[i].BlowDamage);
                if (units[i].TimeToDie)
                {
                    gameObjects.RemoveUnit(units[i]);
                    units.RemoveAt(i);
                    i--;
                }
            }
            coreTotalUpdateTime += ((float)sw.ElapsedTicks) / Stopwatch.Frequency;
            //Global game state update
            CalculateNumberOfUnits();
            CheckEndOfGame();
        }
        //private void ShotsWithUnitsIntersections()
        //{
        //    foreach (Unit unit in units)
        //        if (unit.HP >= 0)
        //        {
        //            Rectangle rect = unit.GetRectangle();
        //            Sphere sphere = rect.BoxSphere;
        //            for (int i = 0; i < shots.shots.Count; i++)
        //            {
        //               // if (unit.ShipType == ShipTypes.Destroyer&&GameVector.Distance(unit.position,shots.shots[i].pos)<10) { }
        //                if (shots.shots[i].GetSphere().Intersects(sphere))
        //                {
        //                    if (unit.ShipType ==  ShipTypes.Destroyer) { }
        //                    if (rect.IntersectsLine(shots.shots[i].pos, shots.shots[i].End))
        //                    {
        //                        //if (unit.Name == "Ship2") { }
        //                        unit.SetHP(unit.HP - shots.shots[i].damage);
        //                        shots.shots.RemoveAt(i);
        //                    }
        //                }
        //            }
        //        }
        //}
        private void DamageAllAround(GameVector pos, float radius, float Damage)
        {
            foreach (Unit unit in units)
            {
                if (GameVector.DistanceSquared(unit.position, pos) <= radius * radius)
                { unit.SetHP(unit.HP - Damage); }
            }
        }
        private void
            UnitIntersections()
        {
            foreach (Unit unit in units)
            {
                if (unit.ShipType == ShipTypes.Destroyer)
                { }
                gameObjects.UpdateUnit(unit);
            }
            foreach (Shots.Shot shot in shots)
            { gameObjects.UpdateShot(shot); }
            for (int i = 0; i < units.Count; i++)
                if (units[i].HP >= 0)
                {
                    if (units[i].ShipType == ShipTypes.Destroyer)
                    { }
                    MiniGameInterfaces.Rectangle rect1 = units[i].GetRectangle();
                    Circle sphere1 = rect1.GetSphere;
                    List<Unit> nearUnits = new List<Unit>();
                    List<Shots.Shot> nearShots = new List<Shots.Shot>();
                    gameObjects.GetNearObjects(units[i].position, 0, out nearUnits, out nearShots);
                    if (units[i].ShipType == ShipTypes.Destroyer)
                    { }
                    foreach (Unit nearUnit in nearUnits)
                        if (nearUnit != units[i])
                        {
                            MiniGameInterfaces.Rectangle rect2 = nearUnit.GetRectangle();
                            if (sphere1.Intersects(rect2.GetSphere))
                            {
                                if (rect1.IntersectsRectangle(rect2))
                                {
                                    float hp1 = units[i].HP;
                                    float hp2 = nearUnit.HP;
                                    units[i].SetHP(hp1 - hp2 * 1.5f);
                                    nearUnit.SetHP(hp2 - hp1 * 1.5f);
                                }
                            }
                        }
                    foreach (Shots.Shot shot in nearShots)
                        if (shot.lifeTime > 0 && !shot.IsChildOf(units[i]))
                        {
                            if (shot.GetSphere().Intersects(sphere1))
                            {
                                if (rect1.IntersectsLine(shot.pos, shot.End))
                                {
                                    //if (unit.Name == "Ship2") { }
                                    units[i].SetHP(units[i].HP - shot.damage);
                                    shots.shots.Remove(shot);
                                    gameObjects.RemoveShot(shot);
                                }
                            }
                        }
                }
                else
                {
                    gameObjects.RemoveUnit(units[i]);
                }
        }
        System.Collections.Generic.List<Unit> units;
        public static GameVector DestroyerSize = new GameVector(25, 25);
        public static GameVector CorvetteSize = new GameVector(80, 20);
        public static GameVector CruiserSize = new GameVector(140, 35);
        public void AddUnits()
        {
            string playerString;
            for (int i = 0; i < players.Count; i++)
            {
                playerString = "Player" + i.ToString() + ".";
                if (Config.Instance.settings.ContainsKey(playerString + "Cruisers") &&
                    Config.Instance.settings.ContainsKey(playerString + "Corvettes") &&
                    Config.Instance.settings.ContainsKey(playerString + "Destroyers"))
                {
                    CreateUnitsForPlayer(i, Convert.ToInt32(Config.Instance.settings[playerString + "Cruisers"]), Convert.ToInt32(Config.Instance.settings[playerString + "Corvettes"]), Convert.ToInt32(Config.Instance.settings[playerString + "Destroyers"]), new GameVector(0, 000));
                }
            }
        }
        private void CreateUnitsForPlayer(int currTeam, int CCruisers, int CCorvettes, int CDestroyers, GameVector pos)
        {
            int sign = currTeam == 0 ? -1 : 1;
            int MaxShipsInLine = 8;
            int CShips = 0;
            float angle = (currTeam > 0) ?  Microsoft.Xna.Framework.MathHelper.Pi : 0;
            GameVector position;
            for (int i = 0; i < CDestroyers; i++)
            {
                position = pos + new GameVector(150 * (CShips / MaxShipsInLine), 150 * (CShips % MaxShipsInLine) + 75 * ((CShips / MaxShipsInLine) % 2));
                position.X += 2000;
                position.X *= sign;
                position.Y += 23;
                units.Add(new Unit(ShipTypes.Destroyer, currTeam, position, angle, "Destroyer -" + i.ToString() + "-"));
                CShips++;
            }
            for (int i = 0; i < CCorvettes; i++)
            {
                position = pos + new GameVector(150 * (CShips / MaxShipsInLine), 150 * (CShips % MaxShipsInLine) + 75 * ((CShips / MaxShipsInLine) % 2));
                position.X += 2000;
                position.X *= sign;
                position.Y += 23;
                units.Add(new Unit(ShipTypes.Corvette, currTeam, position, angle, "Corvette -" + i.ToString() + "-"));
                CShips++;
            }
            for (int i = 0; i < CCruisers; i++)
            {
                position = pos + new GameVector(150 * (CShips / MaxShipsInLine), 150 * (CShips % MaxShipsInLine) + 75 * ((CShips / MaxShipsInLine) % 2));
                position.X += 2000;
                position.X *= sign;
                position.Y += 23;
                units.Add(new Unit(ShipTypes.Cruiser, currTeam, position, angle, "Cruiser -" + i.ToString() + "-"));
                CShips++;
            }
        }
        public void Reset(List<IAI> Players)
        {
            Timing.TimeSpeed = 1.0f;
            Viewer.CameraPosition = new Microsoft.Xna.Framework.Vector3(0, 0, 9000);
            players = Players;
            playersText = new string[players.Count];
            playersTotalUpdateTime = new float[players.Count];
            coreTotalUpdateTime = 0;
            units.Clear();
            shots.Clear();
            gameObjects = new GameObjectsClass();
            AddUnits();
            for (CurrentPlayer = 0; CurrentPlayer < players.Count; CurrentPlayer++)
            {
                playersText[CurrentPlayer] = "";
                players[CurrentPlayer].Init(CurrentPlayer, this);
            }
            gameEnd = false;
            destroyers = new int[players.Count];
            corvettes = new int[players.Count];
            cruisers = new int[players.Count];
            total = new int[players.Count];
            gameEndSoon = false;
        }
        #region IGame Members
        void IGame.SetText(string Text)
        {
            playersText[CurrentPlayer] = Text;
        }
        int IGame.UnitsCount
        {
            get { return units.Count; }
        }
        IUnit IGame.GetUnit(int Index)
        {
            return (IUnit)units[Index];
        }
        int IGame.ShotsCount
        {
            get { return shots.shots.Count; }
        }
        IShot IGame.GetShot(int Index)
        {
            return (IShot)shots.shots[Index];
        }
        public void GetNearUnits(GameVector Position, float Radius, out List<IUnit> NearUnits, out List<IShot> NearShots)
        {
            List<Unit> nearUnits;
            List<Shots.Shot> nearShots;
            GameVector pos = new GameVector(Position.X, Position.Y);
            gameObjects.GetNearObjects(pos, Radius, out nearUnits, out nearShots);
            NearUnits = new List<IUnit>();
            foreach (Unit unit in nearUnits)
                if (GameVector.DistanceSquared(unit.position, pos) < Radius * Radius)
                {
                    NearUnits.Add((IUnit)unit);
                }
            NearShots = new List<IShot>();
            foreach (IShot shot in nearShots)
            {
                NearShots.Add((IShot)shot);
            }
        }
        public float Time
        {
            get
            {
                return timing.NowTime;
            }
        }
        public float TimeElapsed
        {
            get
            {
                return timing.DeltaTime;
            }
        }
        public IDebug GeometryViewer
        {
            get
            {
                return (IDebug)viewer;
            }
        }
        #endregion
    }
    public class Config
    {
        private static Config instance = null;
        public Dictionary<string, string> settings;
        private Config()
        {
            settings = new Dictionary<string, string>();
            StreamReader rd = File.OpenText("MiniGame.ini");
            string st, paramString, valueString;
            int t;
            while (!rd.EndOfStream)
            {
                st = rd.ReadLine();
                if ((st.Length > 0) && (st[0] != '#'))
                {
                    t = st.IndexOf(" ");
                    if (t != -1)
                    {
                        paramString = st.Substring(0, t);
                        valueString = st.Substring(t + 1).Trim();
                        settings.Add(paramString, valueString);
                    }
                }
            }
            rd.Close();
        }
        public static Config Instance
        {
            get
            {
                if (instance == null)
                    instance = new Config();
                return instance;
            }
        }
    }
}
