using System;
using System.Windows;
using System.Windows.Media;


namespace WpfApp
{
    class Robot
    {
        Vector2D randTarget;
        Vector2D catchedTarget = new Vector2D();
        Vector2D p0, p1, p2, p3;
        Vector2D velocity = new Vector2D();
        Vector2D acceleration = new Vector2D();

        Brush colorBody, colorRadar;
        int maxspeed = 3;
        double maxforce = 0.6;
        double angle;
        int length = 10;
        double k1, k2;
        int radius = 70;
        public int state = 0;
        int idCatchedParticle;
        public Queue<int> visitedIndexes = new Queue<int>();
        public bool isDrawRadar;

        public Robot(int k1 = 2, int k2 = 19)
        {
            var x = MainWindow.rnd.Next(MainWindow.width);
            var y = MainWindow.rnd.Next(MainWindow.height);

            p0 = new Vector2D(x, y);
            p1 = new Vector2D(x + length, y);
            p2 = new Vector2D(x, y - 3);
            p3 = new Vector2D(x, y + 3);

            colorBody = Brushes.Black;
            colorRadar = Brushes.Gray;

            this.k1 = k1;
            this.k2 = k2;
        }

        public void Update()
        {
            velocity.Add(acceleration);
            velocity.Limit(maxspeed);
            AddVelocity(velocity);

            acceleration.Mult(0);
        }

        private Vector2D GetRandTarget()
        {
            var x = MainWindow.rnd.Next(MainWindow.width);
            var y = MainWindow.rnd.Next(MainWindow.height);
            return new Vector2D(x, y);
        }

        private void RandomMoving()
        {
            if (randTarget is null)
            {
                randTarget = GetRandTarget();
            }
            else
            {
                var d = p0.Dist(randTarget);

                if (d < radius)
                {
                    // Определение цели движения
                    randTarget = GetRandTarget();
                }
                else
                {
                    var angle = Vector2D.Sub(p0, randTarget);
                    Rotate(angle);

                    var target = Seek(randTarget);
                    target.Mult(maxforce);

                    ApplyForce(target);
                }
            }
        }
        private List<int> GetNearestParticles(List<Particle> particles)
        {
            List<int> indexes = new List<int>();
            foreach (var p in particles)
            {
                var d = p0.Dist(p.pos);
                int id = p.id;

                if (d < radius && !visitedIndexes.Contains(id) && p.state == ParticleState.Down)
                {
                    indexes.Add(id);
                }
            }

            if (indexes.Count == 0) return null;

            return indexes;
        }
        private int CatchTarget(List<Particle> particles)
        {
            List<int> nearestParticles = GetNearestParticles(particles);

            if (nearestParticles is null) return -1;

            int rand_id = MainWindow.rnd.Next(nearestParticles.Count);
            if (idCatchedParticle != nearestParticles[rand_id])
            {
                return nearestParticles[rand_id];
            }

            return -1;
        }
        private void Stop()
        {
            acceleration.Mult(0);
            velocity.Mult(0);
        }
        private int GetParticlesNum(List<Particle> particles)
        {
            int num = 0;
            int id = 0;

            foreach (var p in particles.ToList())
            {
                var d = p0.Dist(p.pos);

                if (d <= radius)
                {
                    num++;
                }
            }

            return num;
        }
        private bool CheckPickingUp(int n)
        {
            double p = Math.Pow(k1 / (k1 + n), 2);

            double rand = MainWindow.rnd.NextDouble();
            if (rand < p)
            {
                return true;
            }

            return false;
        }
        private void PickUp(int id)
        {
            
        }
        private bool CheckDroping(int n)
        {
            double q = Math.Pow(n / (k2 + n), 2);

            double rand = MainWindow.rnd.NextDouble();
            if (rand < q)
            {
                return true;
            }

            return false;
        }

        public void Behaviors(List<Particle> particles)
        {
            switch(state)
            {
                case 0:
                    catchedTarget = null;

                    RandomMoving();

                    idCatchedParticle = CatchTarget(particles);

                    if (idCatchedParticle != -1)
                    {
                        catchedTarget = particles[idCatchedParticle].pos.CopyToVector();
                        randTarget = null;
                        state = 1;
                    }

                    break; // random moving

                case 1:

                    var angle = Vector2D.Sub(p0, catchedTarget);
                    Rotate(angle);

                    var target = Seek(catchedTarget);
                    target.Mult(maxforce);

                    ApplyForce(target);


                    var d = p0.Dist(catchedTarget);
                    if (d < maxspeed)
                    {
                        state = 2;
                    }

                    break; // identify target + moving to take

                case 2: 
                    Stop();
                    state = 3;

                    break; // Stop

                case 3:
                    if (catchedTarget != particles[idCatchedParticle].pos)
                    {
                        // if anyone stolen particle
                        state = 0;
                        break;
                    }
                    
                    int n = GetParticlesNum(particles);
                    bool isCanPickUp = CheckPickingUp(n);

                    if (isCanPickUp)
                    {
                        PickUp(idCatchedParticle);

                        particles[idCatchedParticle].state = ParticleState.Up;
                        catchedTarget = null;

                        state = 4;
                    }
                    else
                    {
                        state = 0;
                    }

                    visitedIndexes.Enqueue(idCatchedParticle); // add to queue

                    break; // pick up

                case 4:

                    RandomMoving();

                    particles[idCatchedParticle].pos = p0.CopyToVector();

                    n = GetParticlesNum(particles);
                    bool isCanDrop = CheckDroping(n);

                    if (isCanDrop)
                    {
                        state = 0;
                        particles[idCatchedParticle].state = ParticleState.Down;
                    }

                    break; // random moving with particle + DropDown
            }
        }

        private void ApplyForce(Vector2D force) => acceleration.Add(force);
        private void AddVelocity(Vector2D velocity)
        {
            p0.Add(velocity);
            p1.Add(velocity);
            p2.Add(velocity);
            p3.Add(velocity);
        }
        private void Rotate(Vector2D target)
        {
            Vector2D direction = Vector2D.Sub(p0, p1);

            angle = target.angleBetween(direction);

            // Направление вращения
            var dir = Math.Sign(direction.Cross(target));


            angle = angle * 180 / Math.PI;

            // Костыль
            double to = 0;
            if (dir > 0)
                to = angle * 0.4;
            else
                to = angle * -0.4;


            //Матрицы трансформации
            double[,] before_points = new double[4, 3] { { p0.X, p0.Y, 1 }, { p1.X, p1.Y, 1 }, { p2.X, p2.Y, 1 }, { p3.X, p3.Y, 1 } };

            Matrix2D mTranslNeg = new Matrix2D();
            mTranslNeg.Translate(-p0.X, -p0.Y);

            Matrix2D mRot = new Matrix2D();
            mRot.Rotate(to);

            Matrix2D mTransl = new Matrix2D();
            mTransl.Translate(p0.X, p0.Y);

            Matrix2D mRes = mTranslNeg * mRot * mTransl; // Перемножение матриц
            var matrixTrans = mRes.ToArray();

            var after_points = Matrix2D.Mult(before_points, matrixTrans);

            p0.X = after_points[0, 0];
            p0.Y = after_points[0, 1];
            p1.X = after_points[1, 0];
            p1.Y = after_points[1, 1];
            p2.X = after_points[2, 0];
            p2.Y = after_points[2, 1];
            p3.X = after_points[3, 0];
            p3.Y = after_points[3, 1];
        }

        // STEER = DESIRED MINUS VELOCITY
        public Vector2D Seek(Vector2D target)
        {
            var desired = Vector2D.Sub(target, p0); // A vector pointing from the location to the target

            // Scale to maximum speed
            desired.SetMag(maxspeed);

            // Steering = Desired minus velocity
            var steer = Vector2D.Sub(desired, velocity);
            steer.Limit(maxforce); // Limit to maximum steering force

            return steer.CopyToVector();
        }

        public void Draw(DrawingContext dc)
        {
            // Draw robot's body
            Point P0 = new Point();
            Point P1 = new Point();
            P0.X = p0.X;
            P0.Y = p0.Y;
            P1.X = p1.X;
            P1.Y = p1.Y;
            dc.DrawLine(new Pen(colorBody, 3), P0, P1);
            Point P2 = new Point();
            Point P3 = new Point();
            P2.X = p2.X;
            P2.Y = p2.Y;
            P3.X = p3.X;
            P3.Y = p3.Y;
            dc.DrawLine(new Pen(colorBody, 3), P2, P3);

            // Draw radar
            if (isDrawRadar)
                dc.DrawEllipse(null, new Pen(colorRadar, 0.4), P0, radius, radius);

            // rand target
            if (randTarget is not null)
            {
                Point p = new Point(randTarget.X, randTarget.Y);
                dc.DrawEllipse(Brushes.Black, null, p, 1, 1);
            }

            // catched target
            if (catchedTarget is not null)
            {
                Point p = new Point(catchedTarget.X, catchedTarget.Y);
                dc.DrawEllipse(Brushes.Yellow, null, p, 3, 3);
            }
        }
    }
}
