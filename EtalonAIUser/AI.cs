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
            get { return "AntonEtalon"; }
        }
        public override string Description
        {
            //this algorithm attacks enemie with effective unit type            
            get { return "Cruis.,Corv.,Destr. attack"; }
        }
        int sign;
        float AngleForward;
        public override void Init(int PlayerNumber, IGame Game)
        {
            if (PlayerNumber == 0) sign = 1;
            else sign = -1;
            AngleForward=1.57f*(1-sign);
            base.Init(PlayerNumber, Game);
            
            
            cruisers= CreateSquadron();
            corvettes= CreateSquadron();
            
            destroyers= CreateSquadron();
            

            for (int i = 0; i < friends.Count; i++)
            {
                if (friends[i].Type == ShipTypes.Cruiser) cruisers.AddUnit(friends[i]);
                else
                {
                    if (friends[i].Type == ShipTypes.Destroyer)
                    {
                      
                        destroyers.AddUnit(friends[i]);
                    }
                    else
                    {
                        
                        corvettes.AddUnit(friends[i]);
                    }
                }
            }
            center = cruisers.formation.GetMassCenter();
            cruisers.SetFormationTypeLine(600);
            destroyers.SetFormationTypeBar(10,200,150);            
            corvettes.SetFormationTypeLine(600);

            corvettes.ArrayOrder(center-GameVector.UnitX*1000*sign, AngleForward);
            cruisers.ArrayOrder(center, AngleForward);
            destroyers.ArrayOrder(center-GameVector.UnitX * 2000*sign, AngleForward);

            timer = new Timer(3);
            
            Stage = 0;
            
        }
        int Stage;
        Timer timer;
        
        GameVector center;
        Squadron cruisers;
        
        Squadron corvettes;
        
        Squadron destroyers;
        public override void Update()
        {
           
            base.Update();
            
            if (Stage == 0)
            {
                if (cruisers.behaviour == Squadron.Behaviours.Waiting && corvettes.behaviour == Squadron.Behaviours.Waiting && destroyers.behaviour == Squadron.Behaviours.Waiting)
                {
                    Stage = 1;
                }
            }
            
            if (Stage == 1)
            {
                if (cruisers.behaviour != Squadron.Behaviours.AttackingClosest)
                    cruisers.AttackClosest();
                if (cruisers.Count == 0)
                    Stage = 2;
            }
            if (Stage == 2)
            {
                if (corvettes.behaviour != Squadron.Behaviours.AttackingClosest)
                    corvettes.AttackClosest();
                if (corvettes.Count == 0)
                    Stage = 3;
            }
            if (Stage == 3)
            {
                if (destroyers.behaviour != Squadron.Behaviours.AttackingClosest)
                    destroyers.AttackClosest();
                if (destroyers.Count == 0)
                    Stage = 4;
            }            
            if (Stage == 4)
            {
                game.SetText("EPIC FAIL!");
            }
        }

        
    }
}
