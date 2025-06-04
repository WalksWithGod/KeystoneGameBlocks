using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genesis
{
	
	// MUST USE A DATA STORE MODEL THAT ALLOWS FAST ITERATION ACROSS ALL "ENTITY" INSTANCES.  THOSE ENTITIES SHOULD THEN
	// MAP TO THAT DATA IN A WAY THAT MAKES IT EASIER TO MODIFY, VIEW, MANUALLY EDIT THE DATA.
	// BUT THE ACTUAL COMPUTATION ALOGORITHMS SHOULD RUN DIRECTLY ON THE DATASTORE AND HAVE GREAT CACHE RESPONSE.
	// PART OF THIS FRANKLY MEANS HAVING FIXED RECORD SIZES.  
	
	// DO RESEARCH ON USING C# FOR FAST SCIENTIFIC NUMBER CRUNCHING
	// DO TEST PROJECTS EXPLORING FAST COMPUTATION AND DATA PROCESSING ON IN MEMORY DBs
	
	// // Keystone.Simulation\StarDigest.cs
	
	
	
    // Leigh Alexander wrote another good piece about the disappointment of GTA 5 on how it feels stagnant and sad now.
    // A commenter commented that part of the problem is the people who made it just maybe don't have much more to say 
    // because they all come from the some place, and work the same jobs, they are themselves stuck in time with GTA
    // so in the context of NPCs in my game and giving our characters real depth, i wanted to add this comment
    // to class Crew {} as a reminder that the crew in our game must be generated in such a way that not only do they have
    // believable histories, but they have believable goals.
    // - i think David Pulver's Vehicles is a good way to think about breaking down characters into parts and then
    //   procedurally generating them based on rules governing each system as well as how those systems can relate to one
    //   another.  
    // - this can / should be a side project we can fiddle with over time.
    // - character goals/dreams - Sims3 had this with goals for being evil genius, olympic athlete, but i think some of these
    //   goals are too suburban/middle class/pretentious/
    //      - for a scifi game we need a better mix of just really small goals (love, family, kids) and
    //        other goals like revolution, a good soldier for a cause, exploring and discovery, riches and fortune, 
    //      and then there are ethics and what are these individuals willng to do to get these dreams?  are there dreams
    //      subserviant to their own character or do the ends justify the means?
    //      - some quirky characters, the one who just wants to be loved, who wants to one day transform himself from fat slob
    //      to a Rock that everybody loves.


    //
    // LIBNOISE.NET model of stringing together functions for modifying a template and in our case
    //              the templates are things like races, species, cultures, factions, individuals, etc
    //
    // birth (these branches should not need to go very deep, but they will accrue modifiers to the relevant
    //        attributes/stats of our generated individual
    //
    //      genetics (argument passed in is base racial range for which are coefficients will apply against)
    //          healthy coefficient (1.0 - 0.0f)
    //              derive physical attributes from parents physical attributes with a small variance modifer representing genetic variation that will either improve or impair their physical appearance
    //
    //          not healthy
    //              physical defect
    //              mental defect
    //              both
    //      nuturing
    //          dysfunction coefficient ( (1.0 - 0.0f) derived from parent's own dysfunction)
    //
    //
    // Adam and Eve (Descend) - Name of our procedural generation program
    //                - we should be able to seed by generation number.  
    //                WAIT, the main thing to remember in terms of procedural generatin of ANYTHING is
    //                that we generate based on parametric functions.  So if we generate a bare bones human
    //                we should start with the basic info about this person so that later if we need to flesh it
    //                out procedurally as a full fledged character, we have all that information.  Thus
    //                The other key here is can we generate stubs for a characters genelogy and still flesh out that 
    //                character such that all of it's relatives and siblings are fleshed out too and they all make sense
    //                together?  Im not sure because descendant character seem derived from ancestors so how can you
    //                flesh out the target character before you've fleshed out his past ancestors?
    //                
    //  - all of the complexity we see in terms of politics, and such we should start as eminating from very very basic
    //    rules that govern competition for resources, strength and power and ability to write the histories that will
    //    shape the future culture
    //
    // I) Genesis - World Creation
    //      Regions 
    //          Climates
    //          Ocean coverage
    //
    //

    // II) Evolution (paramters - original seed, region of the planet, climate)
    //      - single celled life (herbivore, omniovre, carnivore, parasitic, symbiotic)
    //          - multicelluar life
    //              - plant
    //                  - marine
    //                  - terrestial
    //                  - air
    //              - animal
    //                  - protozoa
    //                  - worm
    //                  - insectoid
    //                  - crustacean
    //                  - mamalian
    //                      - winged
    //                      - marine
    //                      - terrestial
    //                  - reptilian
    //                  - amphibian
    //                  - avian
    //
    //              - other
    //
    //
    //         B) Genesis Species
    //              i) Intelligent
    //                      a) Rationality -> Supersitition Coefficient (1.0 Vulcans, 0.0 pyschotic vegetative state)
    //                      b) Creativity (requires some irrationality to gain intuition, otherwise they are brute force min/maxers because they lack imagination)
    //
    //              Races
    //              - base STR
    //              - base INT
    //              - base DEX
    //              - base CON
    //              - base WIL
    //              ii) Non Intelligent

    // III) Culture (parameters - original seed, planet, region)
    //  History (evolve history through wars, natural disasters, predation, disease, etc)
    //  - lineage/genealogy
    //      - father ID , mother ID
    //  - hometown
    //      - culture ID <points to culture table in database so we can infer other aspects of their personality)
    //      - movement history
    //  - ethnicity
    //  - educational history
    //  - factions
    //      - movements (rationality movement, 

    // IV) Individual (parameters - original seed, generation ID, culture ID, 
    //    A) Physical Development
    //  - body type (mesomorph, frail)
    //  - appearance (beautiful, ugly)
    //  - 
    //    B) Mental Development
    //  - Emotional (mature, stunted)
    //  - Intellectual (genius, stunted)
    //  - Spiritual  (devout, nihilistic) (based on the type of belief system, devout can be bad)
    //  - Faith (dogmatic religion, personal virtuisitc, atheist)
    //  - 
    // Advantages
    // Disadvantages
    // Quirks
    // Wealth
    // Status
    // Skills
    // Equipment 
    //
    // Goals, Dreams, Desires
    //
    // Injuries, Illnesses, Disease

    // procedural generation of lifeform bodies
    // http://www.ogre3d.org/addonforums/viewtopic.php?f=3&t=4771
    //The animals should consist of bones
    //We should be able to define via functions how the body part around the bone is made.
    // So only the bones would be connected and manipulated but the body around it would
    // always be created around the bone with the same rules.
    // Also we could auto generate the convex hull of a animal, because simply every
    // bone would get a primitive around it. 
    // 
    // 3. I kind of disagree here, a procedural modelling system should work in a 
    // parametric fashion eg: you never actually move or create a polygon manually, 
    // but rather you set parameters and run them through an algorithm which creates 
    // the result. If the algorithm is good enough, then it should be possible to create 
    // any type of model. Again, see werkkzeug1 for an example of a simple procedural
    // modelling method. I think spore uses the basic principle of a spine to serve as a 
    // base for generating a mesh, again I think this is all done by assigning properties 
    // to bones, and letting the program create a mesh around it. 
    // 


    // Arda - http://gram.cs.mcgill.ca/theses/rudzicz-09-arda.pdf
    // layers upon layers upon layers of computation will yield evolved object models
    // be they lifeforms, belief systems, political factions, etc.  This is like fragment
    // programs for rendering geometry.  The inputs and outputs like in  fragment programs 
    // must match
    public class Operation 
 
    {
        public delegate void PerformOperation(ObjectModel operand1, 
                                              ObjectModel operand2);

        private PerformOperation mOperationDelegate;

        public Operation(PerformOperation operationDelegate)
        {
            if (operationDelegate == null) throw new ArgumentNullException();
            mOperationDelegate = operationDelegate;
        }

        public void Execute(ObjectModel operand1, ObjectModel operand2)
        {
            mOperationDelegate.Invoke(operand1, operand2);
        }

        // return a new more fleshed out ObjectModel based on the source ObjectModel
        // eg apply evolution algorithm, apply populate() algorithm, etc
        // - but what about parameters? let's say we want to run an operation to compute
        //   the resulting population object model when a source object model goes through
        //   a simulated "Famine" operation.  Where do we input parameters for Famine such as the 
        //   duration of the Famine and extant over the region or globe?  Do we have object[] params?
        //   - if so then where do the params come from? is it part of a "climate" object model?
        //     in a sense, an "objectModel" as i'm calling it is really identical to a bag of parameters
        //     that makes up a working set.
        //     - so our OperationLayer logically takes two sets of parameters and performs an operation and then
        //     results in a new ObjectModel (or set of parameters)
        // OjectModel Perform(OjectModel source, ObjectModel climate) ;
        // ObjectModel BeginPerform (OjectModel source, ObjectModel climate);

        // lets say i want generic scripted operations instead of using inheritance to derive
        // new types...  or maybe i attach delegates?  delegates i think are best here because
        // we are talkign potentially a great many different operations
        // however is it true here that our operation cannot return a value?  why not?

    }

    // http://www-cs-students.stanford.edu/~amitp/gameprog.html
    // http://gamesfromwithin.com/data-oriented-design
    // http://molecularmusings.wordpress.com/2013/02/22/adventures-in-data-oriented-design-part-2-hierarchical-data/
    public class ObjectModel // very much like our DomainObject, except these Models start as partial models
                            // that we evolve and grow through our OperationLayer to create a new ObjectModel
    {
        private int mSeed; // seed per object so all objects can be regenned even if starting from random access saved object data 
                           // where we don't necessarily know the starting seed of the entire top to bottom generation

        private Random mRandom; // for proper thread sychronization, every object should use it's own Random class instance
                               // to pass to Operation if applicable
                               // care should be taken that different .net versions (including future) use the same
                               // Random num gen algorithms if you're worried about backwards compatibility after you release your app

        // Attributes[] mAttributes;
        // ObjectModelStore mStore;

        // object GetAttributeValue (string name);
        // void SetAttributeValue (string name, object value);

        public ObjectModel(int seed)
        {
            mRandom = new Random(seed);
        }

        public int Seed { get{return mSeed ;}}


        #region Random Instance Wrapper
        // Operations that need to use random number generation will call the instance of the gen
        // from the ObjectModel passed in.

        // Summary:
        //     Returns a nonnegative random number.
        //
        // Returns:
        //     A 32-bit signed integer greater than or equal to zero and less than System.Int32.MaxValue.
        public virtual int Next()
        {
            return mRandom.Next();
        }
        //
        // Summary:
        //     Returns a nonnegative random number less than the specified maximum.
        //
        // Parameters:
        //   maxValue:
        //     The exclusive upper bound of the random number to be generated. maxValue
        //     must be greater than or equal to zero.
        //
        // Returns:
        //     A 32-bit signed integer greater than or equal to zero, and less than maxValue;
        //     that is, the range of return values ordinarily includes zero but not maxValue.
        //     However, if maxValue equals zero, maxValue is returned.
        //
        // Exceptions:
        //   System.ArgumentOutOfRangeException:
        //     maxValue is less than zero.
        public virtual int Next(int maxValue)
        {
            return mRandom.Next(maxValue);
        }
        //
        // Summary:
        //     Returns a random number within a specified range.
        //
        // Parameters:
        //   minValue:
        //     The inclusive lower bound of the random number returned.
        //
        //   maxValue:
        //     The exclusive upper bound of the random number returned. maxValue must be
        //     greater than or equal to minValue.
        //
        // Returns:
        //     A 32-bit signed integer greater than or equal to minValue and less than maxValue;
        //     that is, the range of return values includes minValue but not maxValue. If
        //     minValue equals maxValue, minValue is returned.
        //
        // Exceptions:
        //   System.ArgumentOutOfRangeException:
        //     minValue is greater than maxValue.
        public virtual int Next(int minValue, int maxValue)
        {
            return mRandom.Next(minValue, maxValue);
        }
        //
        // Summary:
        //     Fills the elements of a specified array of bytes with random numbers.
        //
        // Parameters:
        //   buffer:
        //     An array of bytes to contain random numbers.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     buffer is null.
        public virtual void NextBytes(byte[] buffer)
        {
            mRandom.NextBytes(buffer);
        }
        //
        // Summary:
        //     Returns a random number between 0.0 and 1.0.
        //
        // Returns:
        //     A double-precision floating point number greater than or equal to 0.0, and
        //     less than 1.0.
        public virtual double NextDouble()
        {
            return mRandom.NextDouble();
        }
#endregion
    }

    public class ObjectModelStore // interface for abstract storage of models
                                  // because if we are generating thousands or millions of models, we need to store these
                                  // and different models might need to be stored differently (eg heightmap vs CSV file)
                                  // does a model have to be created with a reference to the store it should use?
                                  // 
                                  // evolutionary histories can be stored, or NOT stored since we can always derive 
                                  // some necessary bit of history procedurally.

                                // stored models can also serve as a starting point for procedural generation
                                // rather than having to start from the lowest element we can start from predefined models

    { 
    }

    // operations 
    // - why operations?
    //   - self contained
    //   - thread safe as long as we keep them self contained
    //   - queable potentially for long operations like doing an errosion filter on a heightmap
    //   - test with a-life automata simulation first?
    public class SpeciesEvolutionOperations
    {
        private static void Test()
        {
            ObjectModel bacteria = new ObjectModel(0);
            ObjectModel food = new ObjectModel(1);

            Operation op = new Operation(Eat);
            op.Execute(bacteria, food);

            ObjectModel mate = new ObjectModel(0);
            Operation breed = new Operation(Breed);
            op.Execute(bacteria, mate);

        }

        public static void Eat(ObjectModel operand1, ObjectModel operand2)
        {
            // consume food off the board

            // update hunger  levels


        }

        public static void Breed(ObjectModel operand1, ObjectModel operand2)
        {

        }

    }

    // a-life for testing

    // AI is inherently procedural... would this system help in that regard?


    // threading - sure we can eneque an operation and wait on the results of that operation
    //             but to avoid recursion, if we want to generate a world that is fully populated down to the individual
    //             then we should do so on a per level basis where we first generate worlds, then only after all worlds
    //             are done do we now advance to generate species, then after species (for any given world at least if not all)
    //             we evolve those species for X generations, then, etc, etc, etc
    //

}
