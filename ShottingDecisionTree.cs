using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;


public class ShottingDecisionTree : MonoBehaviour
{
    //!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\
    //Disclaimer
    //Decision Tree
    //Tested
    //!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\/!\

    static bool inDebug = true;
    static bool inGame = true;
    //Variables
    static int damage = 0;
    static int maxDamageS = 0;
    static int minDamageS = 0;
    static float tX = 0f;
    static float tZ = 0f;
    static float sX = 0f;
    static float sZ = 0f;
    static float dBetween   = 0f;
    static float critic     = 0f; //Probability to happen critic
    static float sRange     = 0f;
    static float defenseRate = 0f; //Probability to don't hit
    static float criticPercentage = 0f; //level 6 
    static string defenceIDTop      = "0";
    static string defenceIDDown     = "0";
    static string defenceIDLeft     = "0";
    static string defenceIDRight    = "0";
    static List<string> defenceIDListArroundTarget      = new List<string>();
    static List<Func<bool>> listCondictionsMainLevel1   = new List<Func<bool>>();
    static List<DTNode> listNodesCondictionsMainLevel1  = new List<DTNode>();
    static List<Func<bool>> listCondictionsMainLevel2   = new List<Func<bool>>();
    static List<DTNode> listNodesCondictionsMainLevel2  = new List<DTNode>();
    static bool tHunkerDown = false;
    //level 4 variables
    static float sDistancePrecentageByHisRange = 0f;
    static DefensesAssetData defensesAssetData;

    //private void Awake()
    //{
    //    defensesAssetData = (DefensesAssetData)ScriptableObject.CreateInstance(typeof(DefensesAssetData));
    //}

    //Node classes
    //First the Node Father class
    abstract class DTNode
    {
        //identification
        public string iD {protected set; get;}

        //action or activation func
        public abstract void Run();
    }

    class DTCondition : DTNode
    {
        //childs from node
        private DTNode ifTrueChild, ifFalseChild;
        //if condiction
        private Func<bool> condiction;

        //for no binary situations
        private List<Func<bool>> listCondictions = new List<Func<bool>>();
        private List<DTNode> listNodesCondictions = new List<DTNode>();

        public override void Run()
        {
            if (inDebug) { Debug.Log("Node ID: \n ID -> " + iD); }

            if (listCondictions.Count != listNodesCondictions.Count)
            {
                if (inDebug) { Debug.Log("ERROR in node condiction the lists are not syncrinised !!\n" +
                    "Condiction list with " + listCondictions.Count + " condictions in list\n" +
                    "Condiction node with " + listNodesCondictions.Count + " nodes in list"); }
            }
            if (listCondictions.Count == 0)
            {
                if (inDebug){ Debug.Log("Conction node with out list"); }
                if (condiction())
                {
                    ifTrueChild.Run();
                }
                else
                {
                    ifFalseChild.Run();
                }
            }
            else
            {
                if (inDebug) { Debug.Log("Conction node with "+ listCondictions .Count +" condictions in list"); }

                //search all the condictions and get his node child
                for (int i = 0; i < listCondictions.Count; i++)
                {
                    if (listCondictions[i]())
                    {
                        listNodesCondictions[i].Run();
                        break;
                    }
                }
            }
        }

        //Contructer
        /// <summary>
        /// Condiction Node
        /// </summary>
        /// <param name="iD"></param>
        /// <param name="condictionFunc"></param>
        /// <param name="childForTrue">Another condiction or action</param>
        /// <param name="childForFalse">Another condiction or action/param>
        public DTCondition(string iD, Func<bool> condictionFunc, DTNode childForTrue, DTNode childForFalse)
        {
            this.iD = iD;
            this.condiction = condictionFunc;
            this.ifTrueChild = childForTrue;
            this.ifFalseChild = childForFalse;
        }
        /// <summary>
        /// ctor for lists
        /// </summary>
        /// <param name="iD">jus an id</param>
        /// <param name="listCondictions">list from condictions</param>
        /// <param name="listNodesCondictions">list of nodes for the condiction in the same index</param>
        public DTCondition(string iD, List<Func<bool>> listCondictions, List<DTNode> listNodesCondictions)
        {
            this.iD = iD;
            this.listCondictions = listCondictions;
            this.listNodesCondictions = listNodesCondictions;
        }
    }

    //Compute the damage it will be the action
    class DTAction : DTNode
    {
        //Variables
        Func<int> intActionFunc;
        Action actionFunc;
        //Func<bool> boolActionFunc2;//could do it in a list but nah
        DTNode nextNode;
        int ctrID = 0;

        public override void Run()
        {
            if (inDebug) { Debug.Log("Node ID: \n ID -> " + iD); }

            actionFunc();
        }

        public DTAction(string iD, Action actionFunc)
        {
            this.iD = iD;
            this.actionFunc = actionFunc;
        }

    }

    //this decorator is a node that only have one child but do action between 
    class DTDecorator : DTNode
    {
        //Variables
        Action actionFunc;
        DTNode nextNode;

        public override void Run()
        {
            if (inDebug) { Debug.Log("Node ID: \n ID -> " + iD); }

            actionFunc();
            nextNode.Run();
        }

        public DTDecorator(string iD, Action actionFunc, DTNode nextNode)
        {
            this.iD = iD;
            this.actionFunc = actionFunc;
            this.nextNode = nextNode;
        }
    }

    //Random node generator not necessery
    class DTFlipCoin : DTCondition
    {
        public DTFlipCoin(string n, DTNode left, DTNode right) :
            base(n, () =>
            {
                return GenerateRandom.GetRandomNumberInt(0, 2) >= 1;
            }, left, right)
        {
            // empty
        }

        public DTFlipCoin(string n,Func<bool> condiction, DTNode ifTrueNode, DTNode ifFalseNode) : 
            base(n, condiction, ifTrueNode, ifFalseNode)
        {

        }
    }

    //Invoque the decision tree
    //->Level 0
    public static int ComputeDamageDT(Transform targetPos, Transform shotterPos, bool targetHunkerDown, float distanceBetweenOBJ, float shooterCriticOffset, float shooterRange, int minDamageShooter, int maxDamageShooter)
    {
        if (inDebug) { Debug.Log("Start Computing Damage Decision Tree"); }

        defensesAssetData = (DefensesAssetData)ScriptableObject.CreateInstance(typeof(DefensesAssetData));

        //variables clear
        damage  = 0;
        tX      = 0f;
        tZ      = 0f;
        sX      = 0f;
        sZ      = 0f;
        dBetween        = 0f;
        tHunkerDown     = false;
        defenceIDTop    = "0";
        defenceIDDown   = "0";
        defenceIDLeft   = "0";
        defenceIDRight  = "0";
        defenceIDListArroundTarget.Clear();
        listCondictionsMainLevel1.Clear();
        listNodesCondictionsMainLevel1.Clear();
        listCondictionsMainLevel2.Clear();
        listNodesCondictionsMainLevel2.Clear();
        critic = 0f;
        sRange = 0f;
        maxDamageS  = 0;
        minDamageS  = 0;
        defenseRate = 0f;
        sDistancePrecentageByHisRange = 0f;
        criticPercentage = 0f;//level 6

        //this is only possible becase the map is all in the same quadrant the 4 Quadrant in the plane x z
        tX          = targetPos.position.x;
        tZ          = targetPos.position.z;
        sX          = shotterPos.position.x;
        sZ          = shotterPos.position.z;
        dBetween    = distanceBetweenOBJ;
        tHunkerDown = targetHunkerDown;
        critic      = shooterCriticOffset;
        sRange      = shooterRange;
        maxDamageS  = maxDamageShooter;
        minDamageS  = minDamageShooter;

        if (tX < 0)
        {
            tX *= -1;
        }

        if (tZ < 0)
        {
            tZ *= -1;
        }

        if (sX < 0)
        {
            sX *= -1;
        }

        if (sZ < 0)
        {
            sZ *= -1;
        }
        if (dBetween < 0)
        {
            dBetween *= -1;
        }

        //convert to percentage
        sDistancePrecentageByHisRange = ThreeSimpleRule(100f, sRange, dBetween);

        //Level -> 7 -> compute Damage 
        //Create Nodes
        DTNode missTargetDamageEnd  = new DTAction("It missed the target", ItMissedTheTarget);
        DTNode hitTargetDamageEnd   = new DTAction("Compute Final Damage", ItHitTheTargetComputeFinalDamage);

        //Level -> 6 -> compute Critic   
        //Create Nodes
        DTNode incrementCritic  = new DTDecorator("Increment the critic", IncrementCritic, hitTargetDamageEnd);
        DTNode ifIsCritic       = new DTCondition("If it is critic", ComputeCrit, incrementCritic, hitTargetDamageEnd);

        //Level -> 5 -> Compute If it hits //Final Damage hora e meia de trabalho  
        //Create Nodes
        DTNode ifHitsTheTarget = new DTCondition("if it hits the target", ComputeHit, ifIsCritic, missTargetDamageEnd);

        //Level -> 4 -> Distance Penalty 
        //Create Nodes
        DTNode sPenaltyPrecentageByHisRangeBetween_0_20     = new DTDecorator("Shooter Penalty Precentage By His Range Between 0 and 20", SPenaltyPrecentageByHisRangeBetween_0_20, ifHitsTheTarget);
        DTNode sPenaltyPrecentageByHisRangeBetween_20_40    = new DTDecorator("Shooter Penalty Precentage By His Range Between 20 and 40", SPenaltyPrecentageByHisRangeBetween_20_40, ifHitsTheTarget);
        DTNode sPenaltyPrecentageByHisRangeBetween_40_60    = new DTDecorator("Shooter Penalty Precentage By His Range Between 40 and 60", SPenaltyPrecentageByHisRangeBetween_40_60, ifHitsTheTarget);
        DTNode sPenaltyPrecentageByHisRangeBetween_60_80    = new DTDecorator("Shooter Penalty Precentage By His Range Between 60 and 80", SPenaltyPrecentageByHisRangeBetween_60_80, ifHitsTheTarget);
        DTNode sPenaltyPrecentageByHisRangeBetween_80_150   = new DTDecorator("Shooter Penalty Precentage By His Range Between 80 and 150", SPenaltyPrecentageByHisRangeBetween_80_150, ifHitsTheTarget);

        //Prepare Condiction list
        listCondictionsMainLevel2.Add(() => sDistancePrecentageByHisRange < 20f);       //Index 1
        listNodesCondictionsMainLevel2.Add(sPenaltyPrecentageByHisRangeBetween_0_20);   //Index 1
        listCondictionsMainLevel2.Add(() => sDistancePrecentageByHisRange < 40f);       //Index 2
        listNodesCondictionsMainLevel2.Add(sPenaltyPrecentageByHisRangeBetween_20_40);  //Index 2
        listCondictionsMainLevel2.Add(() => sDistancePrecentageByHisRange < 60f);       //Index 3
        listNodesCondictionsMainLevel2.Add(sPenaltyPrecentageByHisRangeBetween_40_60);  //Index 3
        listCondictionsMainLevel2.Add(() => sDistancePrecentageByHisRange < 80f);       //Index 4
        listNodesCondictionsMainLevel2.Add(sPenaltyPrecentageByHisRangeBetween_60_80);  //Index 4
        listCondictionsMainLevel2.Add(() => sDistancePrecentageByHisRange < 150f);      //Index 5
        listNodesCondictionsMainLevel2.Add(sPenaltyPrecentageByHisRangeBetween_80_150); //Index 5

        //Condition with all the condiction and actions
        DTNode sPenaltyPrecentageByHisRange = new DTCondition("Thie method has all conditions and actions for the distance penalty", listCondictionsMainLevel2, listNodesCondictionsMainLevel2);

        //Level -> 3 -> filter the defenses 
        //Creat Nodes
        DTNode filterDefenses = new DTDecorator("Filter Defenses", FilterTargetDefenses, sPenaltyPrecentageByHisRange);

        //Level -> 2 -> see if it is hunkerdown
        //Create Nodes
        DTNode isHunkerDownNode     = new DTDecorator("is hunker down Action", IsHunkerDown, filterDefenses);
        DTNode hunkerdownCondiction = new DTCondition("Hunker Down Condiction", () => tHunkerDown, isHunkerDownNode, filterDefenses);

        //create nodes
        //Level -> 1
        //creatNodes
        DTNode condictionTopEnd     = new DTDecorator("condiction -top", getDefenceIDTop, hunkerdownCondiction);
        DTNode condictionDownEnd    = new DTDecorator("condiction Down", getDefenceIDDown, hunkerdownCondiction);
        DTNode condictionLeftEnd    = new DTDecorator("condiction Left", getDefenceIDLeft, hunkerdownCondiction);
        DTNode condictionRightEnd   = new DTDecorator("condiction Right", getDefenceIDRight, hunkerdownCondiction);
        DTNode condictionTopLeft    = new DTDecorator("condiction -top", getDefenceIDTop,condictionLeftEnd);
        DTNode condictionDownRight  = new DTDecorator("condiction Down", getDefenceIDDown,condictionRightEnd);
        DTNode condictionLeftDown   = new DTDecorator("condiction Left", getDefenceIDLeft,condictionDownEnd);
        DTNode condictionRightTop   = new DTDecorator("condiction Right", getDefenceIDRight, condictionTopEnd);

        //prepare condiction list 
        listCondictionsMainLevel1.Add(() => sX < tX && sZ < tZ);  //Index 1
        listNodesCondictionsMainLevel1.Add(condictionTopLeft);    //Index 1
        listCondictionsMainLevel1.Add(() => sX == tX && sZ < tZ); //Index 2
        listNodesCondictionsMainLevel1.Add(condictionLeftEnd);    //Index 2
        listCondictionsMainLevel1.Add(() => sX > tX && sZ < tZ);  //Index 3
        listNodesCondictionsMainLevel1.Add(condictionLeftDown);   //Index 3
        listCondictionsMainLevel1.Add(() => sX > tX && sZ == tZ); //Index 4
        listNodesCondictionsMainLevel1.Add(condictionDownEnd);    //Index 4
        listCondictionsMainLevel1.Add(() => sX > tX && sZ > tZ);  //Index 5
        listNodesCondictionsMainLevel1.Add(condictionDownRight);  //Index 5
        listCondictionsMainLevel1.Add(() => sX == tX && sZ > tZ); //Index 6
        listNodesCondictionsMainLevel1.Add(condictionRightEnd);   //Index 6
        listCondictionsMainLevel1.Add(() => sX < tX && sZ > tZ);  //Index 7
        listNodesCondictionsMainLevel1.Add(condictionRightTop);   //Index 7
        listCondictionsMainLevel1.Add(() => sX < tX && sZ == tZ); //Index 8
        listNodesCondictionsMainLevel1.Add(condictionTopEnd);     //Index 8

        //Start node
        DTNode headNodeGetTDefenses = new DTCondition("Head node, get possible Defenses from the target", listCondictionsMainLevel1, listNodesCondictionsMainLevel1);

        //DTNode treeHead = new DTCondition();
        //Call Level 1 node .run
        headNodeGetTDefenses.Run();

        if (inGame) { Debug.Log("Damage : " + damage); }

        //if damage = -1 it was a miss
        return damage;
    }

    //level -> 1 Node Methods
    // Get defenceIDTop
    static void getDefenceIDTop()
    {
        if (inDebug) { Debug.Log("/!\\Level 1/!\\"); }
        if (inDebug) { Debug.Log("Checking Top Defense"); }

        //possible defenses 
        defenceIDTop = CheckTopDefense(tX, tZ);

        //add to list
        defenceIDListArroundTarget.Add(defenceIDTop);

        if (inDebug) { Debug.Log("ID From Defense -> " + defenceIDTop); }
        if (inDebug) { Debug.Log("the list from target defenses has -> " + defenceIDListArroundTarget.Count + " Defenses"); }

    }

    // Get defenceIDLeft
    static void getDefenceIDLeft()
    {
        if (inDebug) { Debug.Log("/!\\Level 1/!\\"); }
        if (inDebug) { Debug.Log("Checking Left Defense"); }

        //possible defenses 
        defenceIDLeft = CheckLeftDefense(tX, tZ);

        //add to list
        defenceIDListArroundTarget.Add(defenceIDLeft);

        if (inDebug) { Debug.Log("ID From Defense -> " + defenceIDLeft); }
        if (inDebug) { Debug.Log("the list from target defenses has -> " + defenceIDListArroundTarget.Count + " Defenses"); }

    }

    // Get defenceIDDown
    static void getDefenceIDDown()
    {
        if (inDebug) { Debug.Log("/!\\Level 1/!\\"); }
        if (inDebug) { Debug.Log("Checking Down Defense"); }

        //possible defenses 
        defenceIDDown = CheckDownDefense(tX, tZ);

        //add to list
        defenceIDListArroundTarget.Add(defenceIDDown);

        if (inDebug) { Debug.Log("ID From Defense -> " + defenceIDDown); }
        if (inDebug) { Debug.Log("the list from target defenses has -> " + defenceIDListArroundTarget.Count + " Defenses"); }

    }

    // Get defenceIDRight
    static void getDefenceIDRight()
    {
        if (inDebug) { Debug.Log("/!\\Level 1/!\\"); }
        if (inDebug) { Debug.Log("Checking Rigth Defense"); }

        //possible defenses 
        defenceIDRight = CheckRightDefense(tX, tZ);

        //add to list
        defenceIDListArroundTarget.Add(defenceIDRight);

        if (inDebug) { Debug.Log("ID From Defense -> " + defenceIDRight); }
        if (inDebug) { Debug.Log("the list from target defenses has -> " + defenceIDListArroundTarget.Count + " Defenses"); }

    }

    //Chek Top Defenses from obj
    static string CheckTopDefense(float objX, float objZ)
    {
        string defenseID = "0";
        float tempX = 0;
        float tempZ = 0;

        tempX = objX - 1;
        tempZ = objZ;

        if (tempX < 0)
        {
            return defenseID;
        }
        else
        {
            defenseID = GameManagerAirport.CheckingDefencesAtPos(tempX, tempZ);
            return defenseID;
        }

    }

    //Chek Down Defenses from obj
    static string CheckDownDefense(float objX, float objZ)
    {
        string defenseID = "0";
        float tempX = 0;
        float tempZ = 0;

        tempX = objX + 1;
        tempZ = objZ;

        if (tempX > GameManagerAirport.MaxX)
        {
            return defenseID;
        }
        else
        {
            defenseID = GameManagerAirport.CheckingDefencesAtPos(tempX, tempZ);
            return defenseID;
        }

    }

    //Chek Left Defenses from obj
    static string CheckLeftDefense(float objX, float objZ)
    {
        string defenseID = "0";
        float tempX = 0;
        float tempZ = 0;

        tempX = objX;
        tempZ = objZ - 1;

        if (tempZ < 0)
        {
            return defenseID;
        }
        else
        {
            defenseID = GameManagerAirport.CheckingDefencesAtPos(tempX, tempZ);
            return defenseID;
        }
    }

    //Chek Right Defenses from obj
    static string CheckRightDefense(float objX, float objZ)
    {
        string defenseID = "0";
        float tempX = 0;
        float tempZ = 0;

        tempX = objX;
        tempZ = objZ + 1;

        if (tempZ > GameManagerAirport.MaxZ)
        {
            return defenseID;
        }
        else
        {
            defenseID = GameManagerAirport.CheckingDefencesAtPos(tempX, tempZ);
            return defenseID;
        }
    }


    //Level -> 2 -> see if it is hunkerdown
    //Methods
    static void IsHunkerDown()
    {
        if (inDebug) { Debug.Log("/!\\Level 2/!\\"); }
        if (inDebug) { Debug.Log("Target is in hunkerdown mode"); };

        //Increment Defense Rate
        defenseRate += 20f;

        if (inDebug) { Debug.Log("Defense Rate ->" + defenseRate); }

    }

    //Level -> 3 -> filter the defenses 
    //Methods
    /// <summary>
    /// This Method is used for filter the best defense from the target and "return it"
    /// </summary>
    /// <returns></returns>
    static void FilterTargetDefenses()
    {
        if (inDebug) { Debug.Log("/!\\Level 3/!\\"); }
        if (inDebug) { Debug.Log("Cheking targets Defenses"); };

        //temp variable
        float tempDefenseRate = 0f;

        if (defenceIDListArroundTarget.Count == 0)
        {
            if (inDebug) { Debug.Log("It has no Defenses"); };
        }
        else
        {
            if (inDebug) { Debug.Log("It has " + defenceIDListArroundTarget.Count + " Defenses"); };

            //search list of defenses from target
            foreach (string defenseID in defenceIDListArroundTarget)
            {
                if (inDebug) Debug.Log("Defenses ID->" + defenseID);
                if (inDebug) { Debug.Log("Teste from scriptable obj list count from list->"+ defensesAssetData.defensiveAssets.Count); }
                //search list of defenses from list from all defences in game to see his rate by id 
                foreach (var defenceScriptable in defensesAssetData.defensiveAssets) //!\\ watch out it can be needed to instanceate the scriptable obj
                {
                    //if (inDebug) { Debug.Log("Enter in foreach"); }

                    //if id is equal and the defenserate is lesser then that defence rate is higher so is the one that will be used (could use an dictionary but nhe)
                    if (defenceScriptable.matrixCharID == defenseID && tempDefenseRate < defenceScriptable.defenseRate)
                    {
                        tempDefenseRate = defenceScriptable.defenseRate;
                    }
                }
            }
        }

        //increment the defernse rate 
        defenseRate += tempDefenseRate;

        if (inDebug) { Debug.Log("Defenses"); }
        if (inDebug) { Debug.Log("Defense rate from id->" + tempDefenseRate); }
        if (inDebug) { Debug.Log("Final defense rate->" + defenseRate); }

    }

    //Level -> 4 -> distance penalty
    //Methods
    //Shooter Penalty between percentage range [0 - 20]
    static void SPenaltyPrecentageByHisRangeBetween_0_20()
    {
        critic += 5f;

        if (inDebug) { Debug.Log("/!\\Level 4/!\\"); }
        if (inDebug) { Debug.Log("Distance percentage: " + sDistancePrecentageByHisRange); }
        if (inDebug) { Debug.Log("Defense Rate: " + defenseRate); }
        if (inDebug) { Debug.Log("Critic Rate: " + critic); }

    }
    //Shooter Penalty between percentage range ]20 - 40]
    static void SPenaltyPrecentageByHisRangeBetween_20_40()
    {
        defenseRate += 7f;

        if (inDebug) { Debug.Log("/!\\Level 4/!\\"); }
        if (inDebug) { Debug.Log("Distance percentage: " + sDistancePrecentageByHisRange); }
        if (inDebug) { Debug.Log("Defense Rate: " + defenseRate); }

    }
    //Shooter Penalty between percentage range ]40 - 60]
    static void SPenaltyPrecentageByHisRangeBetween_40_60()
    {
        defenseRate += 14f;

        if (inDebug) { Debug.Log("/!\\Level 4/!\\"); }
        if (inDebug) { Debug.Log("Distance percentage: " + sDistancePrecentageByHisRange); }
        if (inDebug) { Debug.Log("Defense Rate: " + defenseRate); }

    }
    //Shooter Penalty between percentage range ]60 - 80]
    static void SPenaltyPrecentageByHisRangeBetween_60_80()
    {
        defenseRate += 21f;

        Debug.Log("/!\\Level 4/!\\");
        Debug.Log("Distance percentage: " + sDistancePrecentageByHisRange);
        Debug.Log("Defense Rate: " + defenseRate);

    }
    //Shooter Penalty between percentage range ]80 - "150"]
    static void SPenaltyPrecentageByHisRangeBetween_80_150()
    {
        defenseRate += 35f;

        if (inDebug) { Debug.Log("/!\\Level 4/!\\"); }
        if (inDebug) { Debug.Log("Distance percentage: " + sDistancePrecentageByHisRange); }
        if (inDebug) { Debug.Log("Defense Rate: " + defenseRate); }

    }

    //Level -> 5 -> If hits the target
    //Methods
    //Calculate if it hits
    static bool ComputeHit()
    {
        bool tempHit = false;
        float veredictNumber = 0f;

        veredictNumber = GenerateRandom.GetRandomNumberFloat(0f, 100f);

        if (veredictNumber >= defenseRate)
        {
            tempHit = true;
        }
        else
        {
            tempHit = false;
        }

        if (inDebug) { Debug.Log("/!\\Level 5/!\\"); }
        if (inDebug) { Debug.Log("Computing Hit"); }
        if (inDebug) { Debug.Log("Defense Rate -> " + defenseRate); }
        if (inDebug) { Debug.Log("veredict Number-> " + veredictNumber); }
        if (inDebug) { Debug.Log("is hit -> " + tempHit); }

        return tempHit;
    }

    //Level -> 6 -> If it is critic
    //Methods
    //compute if it is an critical
    static bool ComputeCrit()
    {
        bool tempCrit = false;
        float veredictNumber = 0f;

        veredictNumber = GenerateRandom.GetRandomNumberFloat(0f, 100f);

        if (veredictNumber > critic)
        {
            tempCrit = false;
        }
        else
        {
            tempCrit = true;
        }

        if (inDebug) { Debug.Log("/!\\Level 6/!\\"); }
        if (inDebug) { Debug.Log("Computing Critic Validation"); }
        if (inDebug) { Debug.Log("Critic Rate-> " + critic); }
        if (inDebug) { Debug.Log("veredict Number-> " + veredictNumber); }
        if (inDebug) { Debug.Log("is crit -> " + tempCrit); }

        return tempCrit;
    }

    static void IncrementCritic()
    {
        criticPercentage = 0.25f;

        if (inDebug) { Debug.Log("/!\\Level 6/!\\"); }
        if (inDebug) { Debug.Log("Incrementing Critic -> " + criticPercentage); }

    }

    //Level -> 7 -> Compute final damage
    //Methods
    //If it missed the target, if it missed the target it will return -1
    static void ItMissedTheTarget()
    {
        //missed the target so the damage it will be -1
        damage = -1;

        if (inDebug) { Debug.Log("/!\\Level 7/!\\"); }
        if (inDebug) { Debug.Log("It missed the target"); }
        if (inGame) { Debug.Log("Damage -> " + damage); }
    }

    //Computing the damage when it hit the target
    static void ItHitTheTargetComputeFinalDamage()
    {
        //variables 
        float tempDamage = 0;

        //get the damage 
        tempDamage = GenerateRandom.GetRandomNumberInt(minDamageS, maxDamageS);

        //get damage in interval
        damage = (int)(tempDamage + tempDamage * criticPercentage);

        if (inDebug) { Debug.Log("/!\\Level 7/!\\"); }
        if (inDebug) { Debug.Log("It was an hit"); }
        if (inDebug) { Debug.Log("It was an critical -> " + criticPercentage); }
        if (inDebug) { Debug.Log("TempDamage -> " + tempDamage); }
        if (inGame) { Debug.Log("Damage -> " + damage); }

    }

    //Compute Decision Tree AI 
    public static int ComputeDamageAI(Transform targetPos, Transform shotterPos, bool targetHunkerDown, float distanceBetweenOBJ, float shooterCriticOffset, float shooterRange, int minDamageShooter, int maxDamageShooter)
    {
        if (inDebug)
        {
            Debug.Log("Start Computing Damage Decision Tree");
        }

        //variables
        damage = 0;
        tX = 0f;
        tZ = 0f;
        sX = 0f;
        sZ = 0f;
        dBetween = 0f;
        tHunkerDown = false;
        defenceIDTop = "0";
        defenceIDDown = "0";
        defenceIDLeft = "0";
        defenceIDRight = "0";
        defenceIDListArroundTarget.Clear();
        critic = 0f;
        sRange = 0f;
        maxDamageS = 0;
        minDamageS = 0;
        defenseRate = 0f;

        //this is only possible becase the map is all in the same quadrant the 4� Quadrant in the plane x z
        tX = targetPos.position.x;
        tZ = targetPos.position.z;
        sX = shotterPos.position.x;
        sZ = shotterPos.position.z;
        dBetween = distanceBetweenOBJ;
        tHunkerDown = targetHunkerDown;
        critic = shooterCriticOffset;
        sRange = shooterRange;
        maxDamageS = maxDamageShooter;
        minDamageS = minDamageShooter;

        if (tX < 0)
        {
            tX *= -1;
        }

        if (tZ < 0)
        {
            tZ *= -1;
        }

        if (sX < 0)
        {
            sX *= -1;
        }

        if (sZ < 0)
        {
            sZ *= -1;
        }
        if (dBetween < 0)
        {
            dBetween *= -1;
        }

        damage = FirstLevel(tX, tZ, sX, sZ);

        //if damage = -1 it was a miss
        return damage;
    }

    //First Level 
    //Compute in 2D possible Defences by Shotter Pos and Target Pos 
    static int FirstLevel(float targetX, float targetZ, float shotterX, float shotterZ)
    {
        if (inDebug)
        {
            Debug.Log("/!\\First Level/!\\");
        }

        //theere is 8 Possibilities/Directions for the shotter pos relatively to the Target Pos 
        if (shotterX < targetX && shotterZ < targetZ)
        {
            //possible defenses 
            defenceIDTop = CheckTopDefense(targetX, targetZ);
            defenceIDLeft = CheckLeftDefense(targetX, targetZ);

            //add to list
            defenceIDListArroundTarget.Add(defenceIDTop);
            defenceIDListArroundTarget.Add(defenceIDLeft);

            if (defenceIDTop == "0" && defenceIDLeft == "0")
            {
                //more crit
                critic += 40f;

                //chyeck hunker down
                HunkerdownEvaluation();
            }
            else
            {
                //Get the best defense Rate
                GetBestDefenseRateArroundTarget();

                //Check HunkerDown
                HunkerdownEvaluation();
            }
        }
        else if (shotterX == targetX && shotterZ < targetZ)
        {
            //possible defenses 
            defenceIDLeft = CheckLeftDefense(targetX, targetZ);

            //add to list
            defenceIDListArroundTarget.Add(defenceIDLeft);


            if (defenceIDLeft == "0")
            {
                //more crit
                critic += 40f;
                
                //check HunkerDown
                HunkerdownEvaluation();
            }
            else
            {
                //Get the best defense Rate
                GetBestDefenseRateArroundTarget();

                //Check HunkerDown
                HunkerdownEvaluation();
            }
        }
        else if (shotterX > targetX && shotterZ < targetZ)
        {
            //possible defenses 
            defenceIDLeft = CheckLeftDefense(targetX, targetZ);
            defenceIDDown = CheckDownDefense(targetX, targetZ);

            //add to list
            defenceIDListArroundTarget.Add(defenceIDDown);
            defenceIDListArroundTarget.Add(defenceIDLeft);


            if (defenceIDLeft == "0" && defenceIDDown == "0")
            {
                //more crit
                critic += 40f;

                //check HunkerDown
                HunkerdownEvaluation();
            }
            else
            {
                //Get the best defense Rate
                GetBestDefenseRateArroundTarget();

                //Check HunkerDown
                HunkerdownEvaluation();
            }
        }
        else if (shotterX > targetX && shotterZ == targetZ)
        {
            //possible defenses 
            defenceIDDown = CheckDownDefense(targetX, targetZ);


            //add to list
            defenceIDListArroundTarget.Add(defenceIDDown);

            if (defenceIDDown == "0")
            {
                //more crit
                critic += 40f;

                //check HunkerDown
                HunkerdownEvaluation();
            }
            else
            {
                //Get the best defense Rate
                GetBestDefenseRateArroundTarget();

                //Check HunkerDown
                HunkerdownEvaluation();
            }

        }
        else if (shotterX > targetX && shotterZ > targetZ)
        {
            //possible defenses 
            defenceIDRight = CheckRightDefense(targetX, targetZ);
            defenceIDDown = CheckDownDefense(targetX, targetZ);

            //add to list
            defenceIDListArroundTarget.Add(defenceIDDown);
            defenceIDListArroundTarget.Add(defenceIDRight);

            if (defenceIDRight == "0" && defenceIDDown == "0")
            {
                //more crit
                critic += 40f;

                //check HunkerDown
                HunkerdownEvaluation();
            }
            else
            {
                //Get the best defense Rate
                GetBestDefenseRateArroundTarget();

                //Check HunkerDown
                HunkerdownEvaluation();
            }

        }
        else if (shotterX == targetX && shotterZ > targetZ)
        {
            //possible defenses 
            defenceIDRight = CheckRightDefense(targetX, targetZ);

            //add to list
            defenceIDListArroundTarget.Add(defenceIDRight);

            if (defenceIDRight == "0")
            {
                //more crit
                critic += 40f;

                //check HunkerDown
                HunkerdownEvaluation();
            }
            else
            {
                //Get the best defense Rate
                GetBestDefenseRateArroundTarget();

                //Check HunkerDown
                HunkerdownEvaluation();
            }

        }
        else if (shotterX < targetX && shotterZ > targetZ)
        {
            //possible defenses 
            defenceIDRight = CheckRightDefense(targetX, targetZ);
            defenceIDTop = CheckTopDefense(targetX, targetZ);

            //add to list
            defenceIDListArroundTarget.Add(defenceIDTop);
            defenceIDListArroundTarget.Add(defenceIDRight);

            if (defenceIDRight == "0" && defenceIDTop == "0")
            {
                //more crit
                critic += 40f;

                //check HunkerDown
                HunkerdownEvaluation();
            }
            else
            {
                //Get the best defense Rate
                GetBestDefenseRateArroundTarget();

                //Check HunkerDown
                HunkerdownEvaluation();
            }
        }
        else if (shotterX < targetX && shotterZ == targetZ)
        {
            //possible defenses 
            defenceIDTop = CheckTopDefense(targetX, targetZ);

            //add to list
            defenceIDListArroundTarget.Add(defenceIDTop);

            if ( defenceIDTop == "0")
            {
                //more crit
                critic += 40f;

                //check HunkerDown
                HunkerdownEvaluation();
            }
            else
            {
                //Get the best defense Rate
                GetBestDefenseRateArroundTarget();

                //Check HunkerDown
                HunkerdownEvaluation();
            }
        }

        return damage;
    }

    //Check the highest defense nearby
    static void GetBestDefenseRateArroundTarget()
    {
        //temp variable
        float tempDefenseRate = 0f;

        //search list of defenses from target
        foreach (var defenseID in defenceIDListArroundTarget)
        {
            Debug.Log("->" + defenseID);
            //search list of defenses from list from all defences in game to see his rate by id 
            foreach (var defenceScriptable in GameManagerAirport.DefensesAssetData.defensiveAssets)
            {
                //if id is equal and the defenserate is lesser then that defence rate is higher so is the one that will be used (could use an dictionary but nhe)
                if (defenceScriptable.matrixCharID == defenseID && tempDefenseRate < defenceScriptable.defenseRate)
                {
                    tempDefenseRate = defenceScriptable.defenseRate;
                }
            }
        }

        defenseRate += tempDefenseRate;

        Debug.Log("->" + defenseRate);
    }

    //SecondLevel 
    static void HunkerdownEvaluation()
    {
        if (inDebug)
        {
            Debug.Log("/!\\Second Level/!\\");
        }

        if (tHunkerDown)
        {
            defenseRate += 20f;
        }

        //3 Level distance penalty
        distancePenalty();
    }

    //Third Level 
    static void distancePenalty()
    {

        //tempVariables
        float sDistancePrecentageByHisRange = 0f;

        //convert to percentage
        sDistancePrecentageByHisRange = ThreeSimpleRule(100f, sRange, dBetween);

        if (sDistancePrecentageByHisRange < 20f)
        {
            critic += 5f;
        }
        else if (sDistancePrecentageByHisRange < 40f)
        {
            defenseRate += 7f;
        }
        else if (sDistancePrecentageByHisRange < 60f)
        {
            defenseRate += 14f;
        }
        else if (sDistancePrecentageByHisRange < 80f)
        {
            defenseRate += 21f;
        }
        else
        {
            defenseRate += 28f;
        }

        Debug.Log("/!\\Third Level/!\\");
        Debug.Log("Distance percentage: "+ sDistancePrecentageByHisRange);
        Debug.Log("Defense Rate: " + defenseRate);

        //4th Level
        ComputeDamage();
    }

    //3simple rule
    static float ThreeSimpleRule(float maxX, float maxY, float y)
    {
        return ((maxX * y) / maxY);
    }

    //Fourth Level
    static void ComputeDamage()
    {
        Debug.Log("/!\\Third Level/!\\");

        //temp variavel
        bool tempHit = false;
        bool tempCrit = false;
        float tempDamage = 0;
        //int veredict = 0;

        //calculate if it hits
        tempHit = ComputeHit();

        if (tempHit)
        {
            //calculate the if it will be a critic 
            tempCrit = ComputeCrit();

            //calculate what damage will be 
            tempDamage = GenerateRandom.GetRandomNumberInt(minDamageS, maxDamageS);

            if (tempCrit)
            {
                damage = (int)(tempDamage * 0.25f);
            }
            else
            {
                damage = (int)tempDamage;
            }
        }
        else
        {
            damage = -1;
        }

        Debug.Log("It was an hit -> " + tempHit);
        Debug.Log("It was an critical -> " + tempCrit);
        Debug.Log("TempDamage -> " + tempDamage);
        Debug.Log("Damage -> " + damage);

    }
}
