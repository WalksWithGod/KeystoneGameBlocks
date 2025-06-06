using System;

class NBody {
   public static void Main(String[] args) {
      int n = 10000;
      if (args.Length > 0) n = Int32.Parse(args[0]);

      NBodySystem bodies = new NBodySystem();

      Console.WriteLine("{0:f9}",bodies.Energy());
      for (int i=0; i<n; i++)
         bodies.Advance(0.01);
      Console.WriteLine("{0:f9}",bodies.Energy());
   }
}


class NBodySystem {
   private Body[] bodies;

   public NBodySystem() {
      bodies = new Body[]{
         Body.Sun(),
         Body.Jupiter(),
         Body.Saturn(),
         Body.Uranus(),
         Body.Neptune()
      };

      double px = 0.0;
      double py = 0.0;
      double pz = 0.0;
      foreach (Body body in bodies) {
         px += body.vx * body.mass;
         py += body.vy * body.mass;
         pz += body.vz * body.mass;
      }
      bodies[0].OffsetMomentum(px,py,pz);
   }

   public void Advance(double dt) {
      double dx, dy, dz, distance, mag;

      for (int i=0; i < bodies.Length; i++) {
         for (int j=i+1; j < bodies.Length; j++) {
            dx = bodies[i].x - bodies[j].x;
            dy = bodies[i].y - bodies[j].y;
            dz = bodies[i].z - bodies[j].z;

            distance = Math.Sqrt(dx*dx + dy*dy + dz*dz);
            mag = dt / (distance * distance * distance);

            bodies[i].vx -= dx * bodies[j].mass * mag;
            bodies[i].vy -= dy * bodies[j].mass * mag;
            bodies[i].vz -= dz * bodies[j].mass * mag;

            bodies[j].vx += dx * bodies[i].mass * mag;
            bodies[j].vy += dy * bodies[i].mass * mag;
            bodies[j].vz += dz * bodies[i].mass * mag;
         }
      }

      foreach (Body body in bodies) {
         body.x += dt * body.vx;
         body.y += dt * body.vy;
         body.z += dt * body.vz;
      }
   }

   public double Energy() {
      double dx, dy, dz, distance;
      double e = 0.0;

      for (int i=0; i < bodies.Length; i++) {
         e += 0.5 * bodies[i].mass *
            ( bodies[i].vx * bodies[i].vx
            + bodies[i].vy * bodies[i].vy
            + bodies[i].vz * bodies[i].vz );

         for (int j=i+1; j < bodies.Length; j++) {
            dx = bodies[i].x - bodies[j].x;
            dy = bodies[i].y - bodies[j].y;
            dz = bodies[i].z - bodies[j].z;

            distance = Math.Sqrt(dx*dx + dy*dy + dz*dz);
            e -= (bodies[i].mass * bodies[j].mass) / distance;
         }
      }
      return e;
   }
}


class Body {
   const double PI = 3.141592653589793;
   const double SOLAR_MASS = 4 * PI * PI;
   const double DAYS_PER_YEAR = 365.24;

   public double x, y, z, vx, vy, vz, mass;

   public Body(){}

   internal static Body Jupiter() {
      Body p = new Body();
      p.x = 4.84143144246472090e+00;
      p.y = -1.16032004402742839e+00;
      p.z = -1.03622044471123109e-01;
      p.vx = 1.66007664274403694e-03 * DAYS_PER_YEAR;
      p.vy = 7.69901118419740425e-03 * DAYS_PER_YEAR;
      p.vz = -6.90460016972063023e-05 * DAYS_PER_YEAR;
      p.mass = 9.54791938424326609e-04 * SOLAR_MASS;
      return p;
   }

   internal static Body Saturn() {
      Body p = new Body();
      p.x = 8.34336671824457987e+00;
      p.y = 4.12479856412430479e+00;
      p.z = -4.03523417114321381e-01;
      p.vx = -2.76742510726862411e-03 * DAYS_PER_YEAR;
      p.vy = 4.99852801234917238e-03 * DAYS_PER_YEAR;
      p.vz = 2.30417297573763929e-05 * DAYS_PER_YEAR;
      p.mass = 2.85885980666130812e-04 * SOLAR_MASS;
      return p;
   }

   internal static Body Uranus() {
      Body p = new Body();
      p.x = 1.28943695621391310e+01;
      p.y = -1.51111514016986312e+01;
      p.z = -2.23307578892655734e-01;
      p.vx = 2.96460137564761618e-03 * DAYS_PER_YEAR;
      p.vy = 2.37847173959480950e-03 * DAYS_PER_YEAR;
      p.vz = -2.96589568540237556e-05 * DAYS_PER_YEAR;
      p.mass = 4.36624404335156298e-05 * SOLAR_MASS;
      return p;
   }

   internal static Body Neptune() {
      Body p = new Body();
      p.x = 1.53796971148509165e+01;
      p.y = -2.59193146099879641e+01;
      p.z = 1.79258772950371181e-01;
      p.vx = 2.68067772490389322e-03 * DAYS_PER_YEAR;
      p.vy = 1.62824170038242295e-03 * DAYS_PER_YEAR;
      p.vz = -9.51592254519715870e-05 * DAYS_PER_YEAR;
      p.mass = 5.15138902046611451e-05 * SOLAR_MASS;
      return p;
   }

   internal static Body Sun() {
      Body p = new Body();
      p.mass = SOLAR_MASS;
      return p;
   }

   internal Body OffsetMomentum(double px, double py, double pz) {
      vx = -px / SOLAR_MASS;
      vy = -py / SOLAR_MASS;
      vz = -pz / SOLAR_MASS;
      return this;
   }
}