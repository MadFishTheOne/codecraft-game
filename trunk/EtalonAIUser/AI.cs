using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiniGameInterfaces;
using AINamespace;
namespace EtalonAIUser
{
    public class UserAI :AINamespace.AI
    {
        public override string Author
        {
            get { return "%username%"; }
        }
        public override string Description
        {
            get { return "clusterization showing"; }
        }
        Color[] colors;
        public override void Init(int PlayerNumber, IGame Game)
        {
            base.Init(PlayerNumber, Game);

            //int i;
            //float j;
            //colors = new Color[3 * 15];
            //for (i = 0; i < 15; i++)
            //{
            //    j = (15 - i)/15f;
            //    colors[i * 3 + 0] = new Color(0.4f + j * 0.6f, 0, 0, 1);
            //    colors[i * 3 + 1] = new Color(0, 0.4f + j * 0.6f, 0, 1);
            //    colors[i * 3 + 2] = new Color(0, 0, 0.4f + j * 0.6f, 1);
            //}
            //colors[0] = new Color(0.6f, 0.0f, 0.0f, 1);
            //colors[1] = new Color(0.0f, 0.6f, 0.0f, 1);
            //colors[2] = new Color(0.0f, 0.0f, 0.6f, 1);
            //colors[3] = new Color(0.6f, 0.6f, 0.0f, 1);
            //colors[4] = new Color(0.6f, 0.0f, 0.6f, 1);
            //colors[5] = new Color(0.0f, 0.6f, 0.6f, 1);
            //colors[6] = new Color(0.6f, 0.6f, 0.6f, 1);
            //colors[7] = new Color(0.1f, 0.5f, 0.0f, 1);
            //colors[8] = new Color(0.0f, 0.5f, 0.1f, 1);

            //GameVector BaseVec,v1, v2, v3, v4, v5;
            //BaseVec = friends[0].ControlledUnit.Position;
            //v1 = BaseVec + GameVector.UnitX.Rotate( 250 * 3.14f / 180f) * 3000;
            //v2 = BaseVec + GameVector.UnitX.Rotate( 225 * 3.14f / 180f) * 3000;
            //v3 = BaseVec + GameVector.UnitX.Rotate( 90 * 3.14f / 180f) * 3000;
            //v4 = BaseVec + GameVector.UnitX.Rotate(135 * 3.14f / 180f) * 3000;
            //v5 = BaseVec + GameVector.UnitX.Rotate(180 * 3.14f / 180f) * 3000;
           
            //SquadronColonel sq1, sq2, sq3, sq4, sq5;
            //sq1 = new SquadronColonel(null, game);
            //sq2 = new SquadronColonel(null, game);
            //sq3 = new SquadronColonel(null, game);
            //sq4 = new SquadronColonel(null, game);
            //sq5 = new SquadronColonel(null, game);

            //for (i = 0; i < 15; i++)
            //{ sq1.AddUnit(friends[i]); }
            //for (i = 15; i < 25; i++)
            //{ sq2.AddUnit(friends[i]); }
            //for (i = 25; i < 30; i++)
            //{ sq3.AddUnit(friends[i]); }
            //for (i = 30; i < 39; i++)
            //{ sq4.AddUnit(friends[i]); }
            //for (i = 39; i < 40; i++)
            //{ sq5.AddUnit(friends[i]); }

            //sq1.GoToOrder(v1);
            //sq2.GoToOrder(v2);
            //sq3.GoToOrder(v3);
            //sq4.GoToOrder(v4);
            //sq5.GoToOrder(v5);

            //squadrons.Add(sq1);
            //squadrons.Add(sq2);
            //squadrons.Add(sq3);
            //squadrons.Add(sq4);
            //squadrons.Add(sq5);

            
            foreach (UnitPilot unit in friends)
                unit.AttackClosest();
      
        }

        public override void Update()
        {
           
            base.Update();
            //for (int i=0;i<enemyAnalyzing.squads.Count;i++)
            //{
            //    foreach (IUnit unit in enemyAnalyzing.squads[i].units)
            //    {                    
            //        game.GeometryViewer.DrawCircle(new Circle(unit.Position, 30), colors[i]);
            //    }
            //}
           
        }
    }
}
