// // http://www.youtube.com/watch?v=EtNyQhSNC8k
//         * Lightnings are pretty complex objects but they can be recreated quite easily using very simple and well-known algorithm 
//         * i.e. a middle point method. Even though it is more often used for procedural terrain generation it can be used with success 
//         * for lightning bolts rendering as the video above shows.

//Unfortunately I cannot tell exactly whose idea it originally was (as I found the link accidentally long time ago and it considered Flash) 
//         * but it is not mine for sure

//Ok, without further ado. What you basically have to do is to for each segment (at first you have only one segment specified by 
//         * lightning's start and end points) find its middle point and offset it a little (by some random value smaller than 
//         * maximumOffset value) along normal vector (perpendicular to the direction vector given by normalize(end - start)). 
//         * This way 2 new segments will be created:

//    * segment1 - start: lightning start, end: lightning middle point,
//    * segment2 - start: lightning middle, end: lightning end point.

//Then for each of the new 2 segments the procedure have to be repeated recursively until desired number of iterations or divisions was made.

//But it will give us boring lightning-like object with no branches at all (lightnings definitely have them!). So a small modification has to be 
//         * made. Every time you divide a segment you can also decide (based on the number of branches already added or some probability 
//         * calculations) whether it should be also split, i.e. whether new branch should begin in the point of division. If yes, new segment 
//         * is created pointing in the direction of lightning end point with some small distortion:

//    * branch - start: current middle point, end: current middle point + normalize(current end - current middle) * length * some random offset.

//The length is some random value from range (minBranchLength, currentSegmentLength). Values larger than currentSegmentLength 
//         * would allow creation of very long and therefore unrealistic branches. This new segment (or branch) is then processed the 
//         * same way as others.


//But it still looks a bit boring. So the solution is to render actually not 1 but 2 lightnings at a time (with the very same start and end points). 
//         * If each of them has life time L than:

//   1. at 0 generate lightning1 with alpha1 = 1.0 
//   2. for range (0, L / 2) interpolate alpha1 from 1 to 0.5
//   3. at L / 2 generate lightning2 with alpha2 1.0
//   4. for range (0, L / 2) interpolate alpha2 from 1 to 0.5 and alpha1 from 0.5 to 0
//   5. for range (L / 2, L) interpolate alpha2 from 0.5 to 0

//Now it looks a lot better. But still not good enough. When lightning hits ground everything becomes white for a brief moment (this 
//         * step is not necessary for rendering electricity related effects though). At first I tried setting some emissive values for different 
//         * materials and using HDR but the results were not good enough. So I chose a different approach.

//I render lightning bolts to the offscreen render targets and when compositing them with the frame buffer I set its contents to almost 
//         * completely white for a fraction of a second.

//It is also important to enhance scene contrast a little but that is quite easy and depends on the effect desired. Of course I have still a 
//         * lot of work to do (add some better glow and fine-tune some parameters) but as I said I'm already pretty happy with the results.
namespace Keystone.FX
{
   
    class FXLightning
    {
        
    }
}
