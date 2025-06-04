//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Economy
//{
//	// MUST USE A DATA STORE MODEL THAT ALLOWS FAST ITERATION ACROSS ALL "ENTITY" INSTANCES.  THOSE ENTITIES SHOULD THEN
	// MAP TO THAT DATA IN A WAY THAT MAKES IT EASIER TO MODIFY, VIEW, MANUALLY EDIT THE DATA.
	// BUT THE ACTUAL COMPUTATION ALOGORITHMS SHOULD RUN DIRECTLY ON THE DATASTORE AND HAVE GREAT CACHE RESPONSE.
	// PART OF THIS FRANKLY MEANS HAVING FIXED RECORD SIZES.  
//    public class Economy
//    {

//        private Database db;
//        private Recordset rsProducts;
//        private Recordset rsMarkets;
//        private Recordset rsCompanies;
//        private Recordset rsRegions;
//        private Recordset rsSupplyDemand;
//        private Recordset rsResources;
//        private Recordset rsContracts;
//        private Recordset rsBuildQueue;

//        private long nTimePerTurn;
//        private string mvardbName;
//        private string mvarTag;
//        private string vData;


//        public bool Connect()
//        {
//            // //connects to a database and returns TRUE if successful
//            db = DBEngine.Workspaces(0).OpenDatabase(mvardbName, false);
//            return true;
//        }


//        public bool InitTimePerTurn()
//        {
//            Recordset rsTemp;
//            // //this will get set the private scope TimePerTurn variable
//            rsTemp = db.OpenRecordset("SELECT GameTimePerTurn FROM SimData");
//            nTimePerTurn = rsTemp;
//            GameTimePerTurn;
//            rsTemp = null;
//            return true;

//        }

//        public bool InitSupplyDemand() {
//        // //This function is run when the Simulation is started. It goes through all of the Markets
//        //   and syncs the total number of products found with that of the regional Supply Demand Matrix
//        //   Once its synced at simulation start, the build queue will ensure that the two matrices
//        //   remain synced
//        long nProduct;
//        long nRegion;
//        long nQuantity;
//        QueryDef MarketQuery; // our temporary query definition
//        Recordset rsTemp; // our temporary recordset
//        // first lets delete all the records in our SupplyDemand table since we are going to rebuild it
//        db.Execute ("DELETE * FROM SupplyDemand");
//        rsProducts = db.OpenRecordset("SELECT ProductID FROM Products", dbOpenSnapshot);
//        rsRegions = db.OpenRecordset("SELECT RegionID From Regions", dbOpenSnapshot);
//        while (!rsProducts.EOF) {
//            rsRegions.MoveFirst;
//            while (!rsRegions.EOF) {
//                // total the quantities of each type of product in each region
//                MarketQuery = db.CreateQueryDef("", ("SELECT  SUM(Quantity) as Total FROM Markets WHERE RegionID = " + rsRegions), (RegionID + (" AND ProductID = " + rsProducts)), ProductID);
//                // add the quantities to the Supply Demand Matrix
//                rsTemp = MarketQuery.OpenRecordset(dbOpenSnapshot);
//                if (IsNull(rsTemp.Total)) {
//                    db.Execute ("UPDATE SupplyDemand SET Supply = 0 WHERE RegionID = " + rsRegions.RegionID + (" AND ProductID = " + rsProducts.ProductID;
//                }
//                else {
//                    db.Execute;
//                    ("UPDATE SupplyDemand SET Supply = " + rsTemp.Total + (" WHERE RegionID = " + rsRegions.RegionID + (" AND ProductID = " + rsProducts.ProductID;
//                    db.Execute;
//                    ("INSERT INTO SupplyDemand(ProductID, RegionID, Supply, Demand, ResourceModifier, SupplyDemandModifier)" +
//                    " VALUES (" + rsProducts.ProductID + (", " + rsRegions.RegionID + (", " + rsTemp.Total + ", 0, 0, 0)");
//                }
//                //  & rsRegions!RegionID & " AND ProductID = " & rsProducts!ProductID
//                rsRegions.MoveNext;
//            }
//            rsProducts.MoveNext;
//        }
//        rsProducts = null;
//        rsRegions = null;
//        return true;

//    }

//        void CreateNewCompanies()
//        {
//            // //This routine evaluates the supply/demand modifier for each product
//            //   and uses it to determine whether new companies need to be created
//            //   to help meet demand
//            const object LIMIT = 10;
//            // this is our threshold for our SupplyDemand Modifier
//            // //retreive all products who have a strong demand and low supply.  That is
//            //        their supply demand modifier is greater than the LIMIT we decide on.
//            // //TODO: all companies that form are of the Regions Tech Level and NOT higher OR less. So
//            //   it should also skip those items who's tech levels that are too high. I believe its
//            //   ok to produce items of lower tech level though right?
//            rsSupplyDemand = db.OpenRecordset(("SELECT ProductID, RegionID, SupplyDemandModifier FROM SupplyDemand WHERE SupplyDemandModifier > " + LIMIT), dbOpenForwardOnly);
//            while (!rsSupplyDemand.EOF)
//            {
//                AddCompany(rsSupplyDemand.ProductID, rsSupplyDemand.RegionID, -1);
//                rsSupplyDemand.MoveNext;
//            }
//            rsSupplyDemand = null;
//        }

//        void AddCompany(long nProduct, long nRegion, long nParentCompanyID)
//        {
//            long nCompanyID;
//            QueryDef CompanyQuery; // our temporary query definition
//            Recordset rsTemp;
//            string sCompanyName;
//            long nOwnerID;
//            // find the highest ID in our Company table
//            CompanyQuery = db.CreateQueryDef("", "SELECT  MAX(CompanID) as MaxID FROM Companies");
//            // TODO: we should try and then look for any open ID's inbetween rather than just using the
//            //       the highest value...still this is temporary.
//            rsTemp = CompanyQuery.OpenRecordset(dbOpenSnapshot);
//            nCompanyID = rsTemp;
//            (Max + 1);
//            sCompanyName = GetNewCompanyName(nProduct, nRegion, nParentCompanyID);
//            nOwnerID = -1;
//            db.Execute;
//            ("INSERT INTO Companies(CompanyID, Name, OwnerID, RegionID, ProductID ) VALUES ("
//                        + (nCompanyID + (", "
//                        + (sCompanyName + (", "
//                        + (nOwnerID + (","
//                        + (nRegion + (", "
//                        + (nProduct + ")"))))))))));
//            CompanyQuery = null;
//            rsTemp = null;
//        }

//        // TODO: this is totally incomplete
//        private string GetNewCompanyName(long nProduct, long nRegion, long nParentCompany)
//        {
//            // //this function takes the Product being produced and what region its being produced
//            //   and comes up with a name for the company.  Also if it has a parent company, it may use
//            //   part of the parent company's name
//            int i = 0;
//            i += 1;
//            return i.ToString();
//        }

//        void RunCompanyManagementAI()
//        {
//            // //Note this routine is only run when the Company is controlled by the AI
//            // //If a company is user controlled, we skip it.  If the company is AI government
//            //   controlled then the AI might need to take into account the governments overall'
//            //   interests and NOT compete with its own companies
//            // //This routine determines
//            //  1) LEVEL 1 AI Determine if we should expand
//            //     - open new Factories in different regions
//            //     - buy other companies and/or factories
//            //  2) LEVEL 2 AI how much to invest in R&D which can allow increased efficiency and decrease cost of goods
//            //     - calculate our new efficiency rating at some point. Higher efficiency means
//            //  less allocation points needed to make the same good.  This will in turn mean we can
//            //  produce more goods for the same price which will wind up decreasing the final price
//            //  of each unit of good.  To stay competivie, companies will need to increase their efficiency
//            //  bigger companies will benefit more by doing this.
//            //     - tech level of good / resource being produced compared to
//            //       the tech level of the company should also impact efficiency?
//            //  - how much more Automation to invest in
//            //     note - can only increase this so much each turn
//            //     note - Automation allows a company to have more equiptment with less workers to run them
//            //            level 10 automation for instance will allow 100 equiptment per 1 person
//            //  - how much more Equiptment to invest in
//            //     note - can only increase this so much each turn
//            // 
//            //  - how much labor to use
//            //     note - can only increase this so much each turn
//            // //
//        }

//        void RunBuildAI()
//        {
//            // //Here each company decides how much to produce and adds it to the
//            //   build queue.  NOTE: this is also based on the length of each turn.
//            //     - how much allocatin points to dedicate to it
//            // Specifics:  Each company has a pool of Production Allocation Points
//            //   - When something is added to the build queue, the Available Allocation
//            //     points is reduced and the Committed is increased by the same amount.
//            //   - When something is finished producing, the Committed points is released
//            //     and the Available is increased by the same amount.
//            //   - If at any time the Total Points is < then the Available + Committed, then
//            //     the Available will be reduced first.  If Available = 0 and Total is still
//            //     less than Committed, then we start removing items from the Build Queue up to
//            //     the difference in points.  At that point Committed must be <= Total Points.
//            //  IMPORTANT: When something is added to the queue, the supplies
//            //   needed to produce it are deducted from both the Markets and the SupplyDemand matrix
//            //   for the region.
//            //   If the item is later canceled from the queue, the "cancel" routine
//            //   will handle the refunding of supplies back to the Market and to the SupplyDemand matrix.
//            //  Primary goal of the Build AI when picking stuff to produce is of course
//            //   to maximize profits.
//            const int AI = -1; // the company is controlled by the AI
//            const int AI_GOVERNMENT = 0; // the company is controlled by an AI controlled Government
//            const int RESOURCE = 0;
//            const int FINISHEDGOOD = 1;
//            long nProductID; // the ID of the good/resource/etc that is being produced
//            float nOccurrence; // actual quantity of a resource in a region
//            byte nDensity; // value is only from 1 to 20
//            byte nPurity; // value is only from 1 to 20
//            long nPointCost;
//            byte nProductType;
//            long nCompanyID;
//            int nSup1;
//            int nSup2;
//            int nSup3;
//            int nSup4;
//            int nSup5;
//            long nMax;
//            //  the maximum number of product we can produce this turn
//            Recordset rsTemp;
//            Recordset rsSupplyReqts;
//            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
//            // retreive all of our companies
//            rsCompanies = db.OpenRecordset("SELECT Companies.CompanyID, Companies.OwnerID, Companies.ProductID, Products.Type, Products.PointCost" +
//                " FROM Companies LEFT OUTER JOIN Products WHERE Companies.ProductID = Products.ProductID", dbOpenForwardOnly);
//            while (!rsCompanies.EOF)
//            {
//                nProductID = rsCompanies.ProductID;
//                nProductType = rsCompanies.Type;
//                nPointCost = rsCompanies.PointCost;
//                nCompanyID = rsCompanies.CompanyID;
//                switch (rsCompanies.OwnerID)
//                {

//                    case AI:
//                        if ((nProductType == RESOURCE))
//                        {
//                            // //first based on the Occurrence Density and Purity
//                            //   we need to figure out the max that one allocation point can produce
//                            rsTemp = db.OpenRecordset(("SELECT Occurrence, Density, Purity FROM ResourceDistribution WHERE ResourceID = " + nProductID), dbOpenForwardOnly);
//                            nOccurrence = rsTemp.Occurrence;
//                            nDensity = rsTemp.DENSITY;
//                            nPurity = rsTemp.Purity;
//                            nMax = GetResourceMineRate(nPointCost, nOccurrence, nDensity, nPurity);
//                        }
//                        else
//                        {
//                            // //get the base number of production points and time needed to complete the item
//                            nMax = nPointCost;
//                            // // retreive the type and amount of supplies needed to make 1 unit of the product
//                            rsSupplyReqts = db.OpenRecordset(("SELECT ProductSupplyReqts.SupplyID, ProductSupplyReqts.Quantity as QuantityNeeded, CompanyInventories" +
//                                ".Quantity as InventoryQuantity FROM ProductSupplyReqts, CompanyInventories WHERE ProductSupplyReqts." +
//                                "ProductID = "
//                                            + (nProductID + (" AND CompanyInventories.CompanyID = "
//                                            + (nCompanyID + " AND CompanyInventories.ProductID = ProductSupplyReqts.ProductID")))), dbOpenForwardOnly);
//                            // // determine the maximum amount we can produce based on supplies on hand
//                            while (!rsSupplyReqts.EOF)
//                            {
//                                nMax = Math.Min(nMax, rsSupplyReqts.InventoryQuantity / rsSupplyReqts.QuantityNeeded);
//                                rsSupplyReqts.MoveNext;
//                            }
//                        }
//                        // // actual supplies level in the Market can now be reduced
//                        // // Supply level in the Supply/Demand matrix must also be reduced next
//                        //    this goes for "resources" as well.
//                        // //determine how many Production Allocation points we are going to assign this
//                        //  perhaps use a random number based on the Supply divided by Demand
//                        // //based on the Supply and Demand levels, determine whether we will increase
//                        //   production or decrease it or leave it at base levels
//                        // // finally add the production item to the build queue
//                        break;
//                    case AI_GOVERNMENT:
//                        // AI controlled Government
//                        // TODO: actually, i should consider seperating AI from the actual Running/Processing
//                        //   of 1 turn in the simulation. hmmm   In any case, the difference between
//                        //   AI and Human, etc, should be seperate?
//                        break;
//                }

//                rsCompanies.MoveNext();
//            }
//            return;

//        }

//        float GetResourceMineRate(long BasePointCost, float Occurrence, byte DENSITY, byte Purity) {
//        // //Based on the occurrence, density and purity, this function returns
//        //   the max amount of the resource that can be pulled out of the ground based on that resources
//        //   base allocation point cost
//        // here is our scale
//        const object Scale1 = 10000;
//        const object Scale2 = 100000;
//        const object Scale3 = 500000;
//        const object Scale4 = 1000000;
//        const object Scale5 = 2000000;
//        const object Scale6 = 3000000;
//        const object Scale7 = 5000000;
//        const object Scale8 = 10000000;
//        const object Scale9 = 15000000;
//        const object Scale10 = 50000000;
//        const object Scale11 = 75000000;
//        const object Scale12 = 100000000;
//        const object Scale13 = 150000000;
//        const object Scale14 = 200000000;
//        const object Scale15 = 350000000;
//        const object Scale16 = 500000000;
//        // 500,000 times population
//        const object Scale17 = 750000000;
//        const object Scale18 = 1000000000;
//        const object Scale19;
//        // TODO: # ... Warning!!! not translated
//        float TempRate;
//        TempRate = BasePointCost;
//        switch (Occurrence)
//        {
//            case Scale1:
//                TempRate = (TempRate * 0.2f);
//            case Scale2:
//                TempRate = (TempRate * 0.3);
//            case Scale3:
//                TempRate = (TempRate * 0.4);
//                case Scale4:
//                TempRate = (TempRate * 0.5);
//                case Scale5:
//                TempRate = (TempRate * 0.6);
//                case Scale6:
//                TempRate = (TempRate * 0.7);
//                case Scale7:
//                TempRate = (TempRate * 0.8);
//                case Scale8:
//                TempRate = (TempRate * 0.9);
//                case Scale9:
//                TempRate = (TempRate * 0.95);
//                case Scale10:
//                TempRate = (TempRate * 1);
//                case Scale11:
//                TempRate = (TempRate * 1.2);
//                case Scale12:
//                TempRate = (TempRate * 1.5);
//                case Scale13:
//                TempRate = (TempRate * 1.75);
//                case Scale14:
//                TempRate = (TempRate * 2);
//                case Scale15:
//                TempRate = (TempRate * 2.5);
//                case Scale16:
//                TempRate = (TempRate * 3);
//                case Scale17:
//                TempRate = (TempRate * 3.5);
//                case Scale18:
//                TempRate = (TempRate * 4);
//                case Scale19:
//                TempRate = (TempRate * 4.5);
//            case > Scale19:    
//                TempRate = (TempRate * 5);
//                break;
//        }
        
//    switch (DENSITY) {
//    case 1:
//        TempRate = (TempRate * 0.2);
//        break;
//    case 2:
//        TempRate = (TempRate * 0.3);
//        break;
//    case 3:
//        TempRate = (TempRate * 0.4);
//        break;
//    case 4:
//        TempRate = (TempRate * 0.5);
//        break;
//    case 5:
//        TempRate = (TempRate * 0.6);
//        break;
//    case 6:
//        TempRate = (TempRate * 0.7);
//        break;
//    case 7:
//        TempRate = (TempRate * 0.8);
//        break;
//    case 8:
//        TempRate = (TempRate * 0.9);
//        break;
//    case 9:
//        TempRate = (TempRate * 0.95);
//        break;
//    case 10:
//        TempRate = (TempRate * 1);
//        break;
//    case 11:
//        TempRate = (TempRate * 1.2);
//        break;
//    case 12:
//        TempRate = (TempRate * 1.5);
//        break;
//    case 13:
//        TempRate = (TempRate * 1.75);
//        break;
//    case 14:
//        TempRate = (TempRate * 2);
//        break;
//    case 15:
//        TempRate = (TempRate * 2.5);
//        break;
//    case 16:
//        TempRate = (TempRate * 3);
//        break;
//    case 17:
//        TempRate = (TempRate * 3.5);
//        break;
//    case 18:
//        TempRate = (TempRate * 4);
//        break;
//    case 19:
//        TempRate = (TempRate * 4.5);
//        break;
//    case 20:
//        TempRate = (TempRate * 5);
//        break;
//    default:
//        messagebox  ("Error");
//        break;
//}
//switch (Purity) {
//    case 1:
//        TempRate = (TempRate * 0.2);
//        break;
//    case 2:
//        TempRate = (TempRate * 0.3);
//        break;
//    case 3:
//        TempRate = (TempRate * 0.4);
//        break;
//    case 4:
//        TempRate = (TempRate * 0.5);
//        break;
//    case 5:
//        TempRate = (TempRate * 0.6);
//        break;
//    case 6:
//        TempRate = (TempRate * 0.7);
//        break;
//    case 7:
//        TempRate = (TempRate * 0.8);
//        break;
//    case 8:
//        TempRate = (TempRate * 0.9);
//        break;
//    case 9:
//        TempRate = (TempRate * 0.95);
//        break;
//    case 10:
//        TempRate = (TempRate * 1);
//        break;
//    case 11:
//        TempRate = (TempRate * 1.2);
//        break;
//    case 12:
//        TempRate = (TempRate * 1.5);
//        break;
//    case 13:
//        TempRate = (TempRate * 1.75);
//        break;
//    case 14:
//        TempRate = (TempRate * 2);
//        break;
//    case 15:
//        TempRate = (TempRate * 2.5);
//        break;
//    case 16:
//        TempRate = (TempRate * 3);
//        break;
//    case 17:
//        TempRate = (TempRate * 3.5);
//        break;
//    case 18:
//        TempRate = (TempRate * 4);
//        break;
//    case 19:
//        TempRate = (TempRate * 4.5);
//        break;
//    case 20:
//        TempRate = (TempRate * 5);
//        break;
//    default:
//        MsgBox ("Error");
//        break;
//}
//return TempRate;
                
//    }

//        double CalcWage(double MinWage, int ResourceValue)
//        {
//            // // The actual wage (the amount of money/goods the worker gets
//            //  from his actual work) is derived from the minimum wage (per day)
//            //  using the Resource value of the provider of the good / service
//            //  in question:
//            return Math.Pow(MinWage * (20 / ResourceValue), 2);
//        }

//        void RunCompanyFinances()
//        {
//            // //This routine calculates the company's operating expenses PER TURN.
//            //  1) Overhead = Workers wages + Maintenace of equiptment
//            //     note - each point of automation doubles cost
//            //  2) stock price
//            //  3) whether we issue more shares
//            const object SECONDS_PER_YEAR = 31536000;
//            // the number of seconds in a year
//            int nTLmod;
//            // tech level modifier for determining maintenance costs
//            float nMinimumWage;
//            float nLaborCost;
//            Recordset rsTemp;
//            float nLaborInventory;
//            byte nTechLevel;
//            long nEquiptment;
//            // //First lets calculate our Equiptment Maintenance Costs per game turn
//            rsTemp = db.OpenRecordset(@"SELECT Companies.CompanyID, Companies.StockPrice, Companies.TechLevel, Companies.Equiptment, Companies.Automation, Companies.Efficiency, Companies.MaintenanceCostPerTurn, Companies.OverHeadCostPerTurn, Companies.RegionID, Regions.MinimumWage FROM Regions RIGHT OUTER JOIN Companies ON Companies.RegionID = Regions.RegionID");
//            while (!rsTemp.EOF)
//            {
//                nMinimumWage = rsTemp.MinimumWage;
//                // nLaborCost = rsTemp!LaborCost
//                // nLaborInventory = rsTemp!LaborInventory
//                // TODO: i think labor should be calced here and not along with BaseCost because
//                //  labor is waged/salaried and get paid each day whether they actually produce something
//                //   or not.  Thats why they would lay people off, to lower overhead.
//                nEquiptment = rsTemp.Equiptment;
//                // get our labor cost per turn
//                nLaborCost = (nMinimumWage * nLaborCost * nLaborInventory) / SECONDS_PER_YEAR * nTimePerTurn;
//                //  TODO: actually labor cost should be based on production to somehow yes?  like what about OT?
//                // get the Tech Level Modifier
//                switch (nTechLevel)
//                {
//                    case 1:
//                        nTLmod = 100;
//                        break;
//                    case 2:
//                        nTLmod = 200;
//                        break;
//                    case 3:
//                        nTLmod = 500;
//                        break;
//                    case 4:
//                        nTLmod = 750;
//                        break;
//                    case 5:
//                        nTLmod = 850;
//                        break;
//                    case 6:
//                        nTLmod = 1000;
//                        break;
//                    case 7:
//                        nTLmod = 5000;
//                        break;
//                    case 8:
//                        nTLmod = 10000;
//                        break;
//                    case 9:
//                        nTLmod = 5000;
//                        break;
//                    case 10:
//                        nTLmod = 2000;
//                        break;
//                    case 11:
//                        nTLmod = 1000;
//                        break;
//                    case 12:
//                        nTLmod = 850;
//                        break;
//                    case 13:
//                        nTLmod = 750;
//                        break;
//                    case 14:
//                        nTLmod = 500;
//                        break;
//                    case 15:
//                        nTLmod = 400;
//                        break;
//                    case 16:
//                        nTLmod = 300;
//                        break;
//                    case 17:
//                        nTLmod = 200;
//                        break;
//                    case 18:
//                        nTLmod = 100;
//                        break;
//                    case 19:
//                        nTLmod = 50;
//                        break;
//                    case 20:
//                        nTLmod = 10;
//                        break;
//                }
//                rsTemp.Edit;
//                rsTemp.MaintenanceCostPerTurn = ((nEquiptment * nTLmod) / SECONDS_PER_YEAR) * nTimePerTurn;
//                // TODO: the amount of automation used should also impact maintenance cost
//                // compute the final overhead cost
//                rsTemp.OverHeadCostPerTurn = rsTemp.MaintenanceCostPerTurn + nLaborCost;
//                // compute our stock price
//                rsTemp.StockPrice = 100;
//                rsTemp.Update;
//                rsTemp.MoveNext;
//            }
//        }

//        void RunContractAI()
//        {
//            // //This routine determines HOW much of certain supplies to buy
//            //   and WHO to buy the supplies from and for what DURATION
//            //   Factors which determine how much to buy is determined
//            //   1) how much we are producing
//            //   2) by the supply and demand of the good(s) we are going to produce
//            //   3) the supply of the resources we are trying to obtain,
//            //   4) the amount of supplies that are already incoming
//            //   5) our current supply levels
//            //   We could say that as a rule, a company tries to have a supply level that is
//            //   randomly between 200% and 1000% of its production output modified by the supply/demand
//            //   ratio.
//            //   Factors which determine WHO is
//            //   0) Type of Company.  Resource producers might not need any supplies
//            //      they just drill/mine/etc
//            //   1) price
//            //   2) who owns the company we are buying from (buy from sister companies first)
//            //   3) location (but from nearby companies first rather than weight for delivery)
//            //   4) cost of the supplies (buy cheapest)
//            //   5) whether they are allowed to buy from them (i.e. not at war with their country)
//            //   6) whether the other company is allowed to sell to them.
//            //  Factors which determine Duration
//            //   1) the main determinant is Supply/Demand modifier of the Supplies we want to buy.
//            //      if its very hard to obtain, then they might try to lock down the arrangement
//            //      with perhaps even a negotiable contract that can be "at market" each turn
//            long i;
//            // loop through all the companies
//            // For i = 1 To UBound(CompanyMatrix)
//            // look only for NON resource producers since these are the only ones that need
//            //  to buy supplies
//            // Next
//        }

//        void RunProductConsumption()
//        {
//            // // This sub goes through all products in all markets and simulates the consumption
//            //    of those goods.  When goods are consumed or "bought" then the companies receive
//            //    revenue for those goods equal to the Quanity bought * RetailPrice
//            //   TODO: figure out factors for determinine amount of consumption of various goods
//            // 
//            // Population - the more people, the more goods get bought
//            // Demand Rating - the higher the demand, the more that will be bought?
//            // Prosperity Rating - the better the people feel about the future, the more they will spend
//            //   Actually i should consider whether maybe all i really need is just a simple
//            //   Population * Demand.  Where Demand of 20 = ~90% and Demand of 1 = 1%
//            //   So if there is 500,000 cars and the demand is 90%, then ~450,000 will be bought.
//            //   the next step is figuring out how to split this demand over various companies who
//            //   may all be offering their produts.  Perhaps a combination of Luck, MarketingAllowance
//            //   and AgeOfCompany, Novelty(fad), etc.
//            //   but as for determining the total amount of a type of good people will buy
//            //    we probably dont even need to wory about Prosperity rating or any other ratings. We
//            //   just need Demand Rating and Population since Demand Rating already takes into account the
//            //   prosperity
//            //  IMPORTANT:  If people want to buy and their is not enough supply, Prosperity should decrease
//            //  and maybe if people cant get food, then people will die each turn.  The amount that dies
//            //  will be based on the amount of time each turn represents
//        }

//        void UpdateDemand()
//        {
//            // //This sub goes through every product in the SupplyDemand table and calculates the demand
//            //   value for each product. The range of values is between 1 and 20 with 1 being extremely low
//            //   demand and 20 being extremely high demand.
//            //  TODO: need to come up with a formula for determining demand
//            //  Prosperity
//            //  Economy
//            //  etc?
//        }

//        void UpdateSupplyDemandModifiers()
//        {
//            long nProdID;
//            // the ID of the good/resource/etc that is being produced
//            long nRegionID;
//            long nPopulation;
//            // get the amount of people living in the region
//            float nResourceModifier;
//            // the resource modifier used to determine base price
//            long nSupplyDemandModifier;
//            float nSupply;
//            // the actual quantity of goods
//            byte nDemand;
//            // value from 1 to 20 that holds the demand rating for the product
//            // //This routine gets each ProductID in the SupplyDemandMatrix
//            //   and updates the "ResourceModifier" AND the "SupplyDemandModifier"
//            //   which is used to determine our base and market prices.
//            // //The resource modifier takes all the resources needed to make the product
//            //   and then divides the Demand by the Supply of the resources in that region in order
//            //   to determine a price
//            // //The SupplyDemandMatrix compares the total Demand for a product within a region against
//            //  the total supply and then produces a modifier between 0.05 and 20.0
//            //  a low demand (1) for an item with high supply (20) would produce .05 supplydemandmodifier
//            //  which would have the effect of lowering the price of the item
//            // retreive a record set containing all products in all regions from the Supply Demand Matrix.
//            // we are joining into our recordset the Population for each region as well
//            rsSupplyDemand = db.OpenRecordset(@"SELECT Regions.Population, SupplyDemand.ProductID, SupplyDemand.RegionID, SupplyDemand.Supply, SupplyDemand.Demand, SupplyDemand.ResourceModifier, SupplyDemand.SupplyDemandModifier FROM SupplyDemand LEFT OUTER JOIN Regions ON Regions.RegionID = SupplyDemand.RegionID", dbOpenDynaset);
//            while (!rsSupplyDemand.EOF)
//            {
//                nProdID = rsSupplyDemand.ProductID;
//                nRegionID = rsSupplyDemand.RegionID;
//                nPopulation = rsSupplyDemand.Population;
//                nSupply = rsSupplyDemand.Supply;
//                nDemand = rsSupplyDemand.Demand;
//                nSupplyDemandModifier = GetSupplyDemandModifier(nPopulation, nDemand, nSupply);
//                rsSupplyDemand.Edit;
//                rsSupplyDemand.SupplyDemandModifier = nSupplyDemandModifier;
//                // retreive a list of all the resources needed to make this product and the supply and demand for them in the region
//                rsResources = db.OpenRecordset("SELECT SupplyDemand.Supply, SupplyDemand.Demand FROM ProductSupplyReqts INNER JOIN SupplyDemand ON Pr" +
//                    "oductSupplyReqts.ProductID = SupplyDemand.ProductID");
//                nResourceModifier = 1;
//                while (!rsResources.EOF)
//                {
//                    nResourceModifier = (nResourceModifier * GetSupplyDemandModifier(nPopulation, rsResources, Demand, rsResources, Supply));
//                    rsResources.MoveNext;
//                }
//                //  finally update our ResourceModifier for this product
//                rsSupplyDemand.ResourceModifier = nResourceModifier;
//                rsSupplyDemand.Update;
//                rsSupplyDemand.MoveNext;
//            }
//            rsSupplyDemand = null;
//            rsResources = null;
//        }

//        float GetSupplyDemandModifier(long Population, byte Demand, float Supply)
//        {
//            // //This function accepts the Population of the region,and the amount of supply and demand
//            //  for a particular good and derives a number between 1 and 20 for the Supply
//            //  the Demand is already sent as a value between 1 and 20 by the routine that
//            //  calculates the demand for products
//            float tempModifier;
//            // Here is our supply scale
//            const object Scale1 = 10;
//            //  10 times population
//            const object Scale2 = 100;
//            const object Scale3 = 500;
//            const object Scale4 = 1000;
//            const object Scale5 = 2000;
//            const object Scale6 = 3000;
//            const object Scale7 = 5000;
//            const object Scale8 = 10000;
//            const object Scale9 = 15000;
//            const object Scale10 = 50000;
//            const object Scale11 = 75000;
//            const object Scale12 = 100000;
//            const object Scale13 = 150000;
//            const object Scale14 = 200000;
//            const object Scale15 = 350000;
//            const object Scale16 = 500000;
//            // 500,000 times population
//            const object Scale17 = 750000;
//            const object Scale18 = 1000000;
//            const object Scale19 = 5000000;
//            tempModifier = (Supply / Population);
//            // check for a demand value that is outside of the range of 1 and 20
//            if ((Demand > 20))
//            {
//                Demand = 20;
//            }
//            else if ((Demand < 1))
//            {
//                Demand = 1;
//            }
//            switch (tempModifier)
//            {
//                case Scale1:
//                    tempModifier = (Demand / 1);
//                case Scale2:
//                    tempModifier = (Demand / 2);
//                case Scale3:
//                    tempModifier = (Demand / 3);
//                case Scale4:
//                    tempModifier = (Demand / 4);
//                case Scale5:
//                    tempModifier = (Demand / 5);
//                case Scale6:
//                    tempModifier = (Demand / 6);
//                case Scale7:
//                    tempModifier = (Demand / 7);
//                    Scale8;
//                    tempModifier = (Demand / 8);
//                    Scale9;
//                    tempModifier = (Demand / 9);
//                    Scale10;
//                    tempModifier = (Demand / 10);
//                    Scale11;
//                    tempModifier = (Demand / 11);
//                    Scale12;
//                    tempModifier = (Demand / 12);
//                    Scale13;
//                    tempModifier = (Demand / 13);
//                    Scale14;
//                    tempModifier = (Demand / 14);
//                    Scale15;
//                    tempModifier = (Demand / 15);
//                    Scale16;
//                    tempModifier = (Demand / 16);
//                    Scale17;
//                    tempModifier = (Demand / 17);
//                    Scale18;
//                    tempModifier = (Demand / 18);
//                    Scale19;
//                    tempModifier = (Demand / 19);
//                    Scale19;
//                    tempModifier = (Demand / 20);
//            }
//            return tempModifier;
//        }

//        void RunBuildQueue() {
//        // //This function uses the BuildQueueMatrix and processes one game turn.  Depending
//        //   on how much time is represented per turn, a certain amount of the Quantity
//        //   that is set to be produced, will be created and then placed in the MarketsMatrix and
//        //   the SupplyDemandMatrix will be updated as well.
//        float nQuantity;
//        float nProductionExpenses;
//        long nPopulation;
//        // holds the population within the region that the good is being produced
//        long nProductID;
//        long nCompanyID;
//        long nRegionID;
//        long nAllocatedProductionPoints;
//        long nTimeAlreadyCompleted;
//        long nTotalTimeNeeded;
//        // On Error GoTo errorhandler
//        rsBuildQueue = db.OpenRecordset("SELECT BuildQueue.*, Companies.RegionID FROM BuildQueue LEFT OUTER JOIN Companies ON  Companies.Compa" +
//            "nyID = BuildQueue.CompanyID", dbOpenDynaset);
//        while (!rsBuildQueue.EOF) {
//            nProductID = rsBuildQueue.ProductID;
//            nCompanyID = rsBuildQueue.CompanyID;
//            nQuantity = rsBuildQueue.Quantity;
//            nRegionID = rsBuildQueue.RegionID;
//            nAllocatedProductionPoints = rsBuildQueue.AllocatedProductionPoints;
//            nTotalTimeNeeded = rsBuildQueue.TimeNeeded;
//            nTimeAlreadyCompleted = rsBuildQueue.TimeCompleted + nTimePerTurn;
//            rsBuildQueue.Edit;
//            rsBuildQueue.TimeCompleted = nTimeAlreadyCompleted;
//            rsBuildQueue.Update;
//            // TODO: the quantity of production allocation points should be factored into
//            //    maybe how many we can actually produce this turn.  Or should this
//            //    be automatically set so that its all built in one turn in the build queue?
//            // //check to see if this product has completed being built.
//            if ((nTimeAlreadyCompleted >= nTotalTimeNeeded)) {
//                // //prepare to add the finished products to the Market.
//                //  If the products are already in the market for the company in that region,
//                //  then we can update the quantities
//                rsMarkets = db.OpenRecordset(("SELECT CompanyID, Quantity FROM Markets WHERE CompanyID = " 
//                                + (nCompanyID + (" AND ProductID = " 
//                                + (nProductID + (" AND RegionID = " + nRegionID))))), dbOpenDynaset);
//                // //if the product exists updated it, else create a new record in the Markets table
//                if (!rsMarkets.EOF) {
//                    rsMarkets.Edit;
//                    rsMarkets.Quantity = nQuantity;
//                    rsMarkets.Update;
//                }
//                else {
//                    db.Execute;
//                    ("INSERT INTO Markets (CompanyID,ProductID, RegionID, Quantity) VALUES ( " 
//                                + (nCompanyID + ("," 
//                                + (nProductID + ("," 
//                                + (nRegionID + ("," 
//                                + (nQuantity + ")"))))))));
//                }
//                // // update the REGIONAL level of "supply" in the Producers region
//                //    in the Supply&Demand table or create it if it doesnt exist
//                rsSupplyDemand = db.OpenRecordset(("SELECT Supply FROM SupplyDemand WHERE ProductID = " 
//                                + (nProductID + (" AND RegionID = " + nRegionID))));
//                if (!rsSupplyDemand.EOF) {
//                    rsSupplyDemand.Edit;
//                    rsSupplyDemand.Supply = rsSupplyDemand.Supply + nQuantity;
//                    rsSupplyDemand.Update;
//                }
//                else {
//                    db.Execute;
//                    ("INSERT INTO SupplyDemand (ProductID, RegionID,Supply) VALUES ( " 
//                                + (nProductID + ("," 
//                                + (nRegionID + ("," 
//                                + (nQuantity + ")"))))));
//                }
//                // //determine our production expenses as a fraction of our total Production
//                //   Capacity per turn
//                rsCompanies = db.OpenRecordset(("SELECT OverheadCostPerTurn, ProductionCapacity, Cash FROM Companies WHERE CompanyID = " + nCompanyID));
//                nProductionExpenses = rsCompanies.OverHeadCostPerTurn * rsCompanies.ProductionCapacity / nAllocatedProductionPoints);
//                // //Reduce Cash by our expenses
//                rsCompanies.Edit;
//                rsCompanies.Cash = rsCompanies.Cash - nProductionExpenses;
//                rsCompanies.Update;
//                // //TODO: Since the items have finished being produced we can remove them from the
//                //   build queue yes? And we must release any Allocation Points back
//                //   to the Company's Available Pool and Decrease the "Committed" Pool.
//            }
//            rsBuildQueue.MoveNext;
//        }
//        return;

//    }

//        void UpdateMarketPrices() {
//        // //This routine updates the Base, Minimum and Market Prices for all goods in all markets
//        float nBasePrice;
//        float nWholeSalePrice;
//        float nRetailPrice;
//        long nRegionID;
//        long nProductID;
//        long nCompanyID;
//        float nProductionExpenses;
//        long nAllocationPointsNeeded;
//        QueryDef TempQuery;
//        Recordset rsTemp;
//        float nAverage;
//        // //first thing that should be done is get our average wholesale price for all goods in the region
//        rsSupplyDemand = db.OpenRecordset("SELECT ProductID, RegionID, AverageWholeSalePrice FROM SupplyDemand");
//        while (!rsSupplyDemand.EOF) {
//            nRegionID = rsSupplyDemand.RegionID;
//            nProductID = rsSupplyDemand.ProductID;
//            TempQuery = db.CreateQueryDef("", ("SELECT AVG(WholeSalePrice) as Average FROM Markets WHERE ProductID = " 
//                            + (nProductID + (" AND RegionID = " + nRegionID))));
//            rsTemp = TempQuery.OpenRecordset;
//            rsSupplyDemand.Edit;
//            if (!IsNull(rsTemp.Average)) {
//                rsSupplyDemand.AverageWholeSalePrice = rsTemp.Average;
//                rsSupplyDemand.Update;
//            }
//            rsSupplyDemand.MoveNext;
//        }
//        // //Base Price per good is the average cost of the supplies needed to make the goods
//        //   plus our Operating Expenses per production point needed to make the good
//        rsMarkets = db.OpenRecordset(@"SELECT Markets.CompanyID, Markets.ProductID, Markets.RegionID, Markets.WholeSalePrice, Markets.RetailPrice, Companies.OverHeadCostPerTurn, SupplyDemand.ResourceModifier, SupplyDemand.SupplyDemandModifier FROM ((Markets LEFT OUTER JOIN Companies ON Companies.CompanyID = Markets.CompanyID) LEFT OUTER JOIN SupplyDemand ON SupplyDemand.ProductID = Markets.ProductID )", dbOpenDynaset);
//        while (!rsMarkets.EOF) {
//            rsResources = db.OpenRecordset("SELECT ProductSupplyReqts.Quantity, SupplyDemand.AverageWholeSalePrice, SupplyDemand.ResourceModifier" +
//                " FROM ProductSupplyReqts INNER JOIN SupplyDemand ON ProductSupplyReqts.ProductID = SupplyDemand.Prod" +
//                "uctID");
//            while (!rsResources.EOF) {
//                nBasePrice = nBasePrice + rsResources.AverageWholeSalePrice * rsResources.Quantity;
//                rsResources.MoveNext;
//            }
//            nBasePrice = nBasePrice + rsMarkets.OverHeadCostPerTurn;
//            // //Wholesale Price is  base price * the demand modifier for the resources
//            db.Execute;
//            ("UPDATE Markets SET WholeSalePrice = " 
//                        + (nBasePrice * rsMarkets.ResourceModifier + (" WHERE Markets.ProductID = " + rsMarkets.ProductID + (" AND Markets.RegionID = " + rsMarkets.RegionID + (" AND Markets.CompanyID = " + rsMarkets.CompanyID;
//            // //Retail Price wholesale price * the demand modifier for the final product
//            db.Execute;
//            ("UPDATE Markets SET RetailPrice = " + rsMarkets.WholeSalePrice * rsMarkets.SupplyDemandModifier + (" WHERE Markets.ProductID = " + rsMarkets.ProductID + (" AND Markets.RegionID = " + rsMarkets.RegionID + (" AND Markets.CompanyID = " + rsMarkets.CompanyID;
//            rsMarkets.MoveNext;
//        }
//    }

//        void RunContracts()
//        {
//            // // A contract is basically a script between companies
//            //    1 company exchanges goods for $$$ with another company
//            // Syntax:
//            // CompanyID1, MarketID, ProductID, Quantity, Amount, CompanyID2, Duration
//            //  here company1 is buying the goods from Company2
//            // //This routine will run all contracts in the matrix.  If the duration is set
//            //  to 1 turn, then the Contract will be removed from the Matrix after its executed
//            long nBuyerID;
//            // id of the Buying company
//            long nSellerID;
//            // id of the Selling company
//            long nQuantity;
//            // 
//            long nMarketID;
//            // the market from which the buyer is getting the goods from
//            long nDuration;
//            long nProductID;
//            float nCostPerUnit;
//            long nRegionID;
//            // retreive our Contracts recordset
//            rsContracts = db.OpenRecordset("SELECT Contracts.*, CompanyInventories.Quantity, Markets.MarketID, Markets.RegionID FROM Contracts, C" +
//                "ompanyInventories, Markets WHERE Contracts.ProductID = CompanyInventories.ProductID AND Contracts.Ma" +
//                "rketID = Markets.MarketID");
//            while (!rsContracts.EOF)
//            {
//                // With...
//                nBuyerID = BuyerID;
//                nSellerID = SellerID;
//                nQuantity = Quantity;
//                nCostPerUnit = CostPerUnit;
//                nProductID = ProductID;
//                nMarketID = MarketID;
//                nRegionID = RegionID;
//                nDuration = Duration;
//                // //now add the supplies that have been bought to the buying companies inventory
//                db.Execute;
//                ("UPDATE CompanyInventories SET Quantity = Quantity "
//                            + (nQuantity + (" WHERE ProductID = "
//                            + (nProductID + (" AND CompanyID = " + nBuyerID)))));
//                // //reduce the inventory of the selling company
//                db.Execute;
//                ("UPDATE CompanyInventories SET Quantity = Quantity "
//                            + ((nQuantity * -1) + (" WHERE ProductID = "
//                            + (nProductID + (" AND CompanyID = " + nSellerID)))));
//                // //now decrease the cash of the buynig company
//                db.Execute;
//                ("UPDATE Companies SET Cash =  Cash "
//                            + (((nQuantity * nCostPerUnit)
//                            * -1) + (" WHERE CompanyID = " + nSellerID)));
//                // //now increase the cash of the selling company
//                db.Execute;
//                ("UPDATE Companies SET Cash =  Cash "
//                            + ((nQuantity * nCostPerUnit) + (" WHERE CompanyID = " + nBuyerID)));
//                // //reduce supply from the selling region
//                db.Execute;
//                ("UPDATE SupplyDemand Set Supply = Supply "
//                            + ((nQuantity * -1) + (" WHERE ProductID = "
//                            + (nProductID + (" AND RegionID = " + nRegionID)))));
//                // //increase supply from the buying region
//                db.Execute;
//                ("UPDATE SupplyDemand Set Supply = Supply "
//                            + (nQuantity + (" WHERE ProductID = "
//                            + (nProductID + (" AND RegionID = " + nRegionID)))));
//                // //factor shipping costs?
//                // TODO:
//                // //sales tax?
//                // TODO:
//                // //tarrif if its imported?
//                // TODO:
//                // //Decrement the counter for that Contract and Delete the contract if the duration then = 0
//                nDuration = (nDuration - 1);
//                if ((nDuration == 0))
//                {
//                    rsContracts.Edit;
//                    rsContracts.Delete;
//                    rsContracts.Update;
//                }
//                else
//                {
//                    rsContracts.Edit;
//                    rsContracts.Duration = nDuration;
//                    rsContracts.Update;
//                }
//                rsContracts.MoveNext;
//            }
//        }
//    }
//}
