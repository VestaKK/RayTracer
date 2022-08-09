using System;
using System.Collections.Generic;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a ray traced scene, including the objects,
    /// light sources, and associated rendering logic.
    /// </summary>
    public class Scene
    {
        private SceneOptions options;
        private ISet<SceneEntity> entities;
        private ISet<PointLight> lights;

        /// <summary>
        /// Construct a new scene with provided options.
        /// </summary>
        /// <param name="options">Options data</param>
        public Scene(SceneOptions options = new SceneOptions())
        {
            this.options = options;
            this.entities = new HashSet<SceneEntity>();
            this.lights = new HashSet<PointLight>();
        }

        /// <summary>
        /// Add an entity to the scene that should be rendered.
        /// </summary>
        /// <param name="entity">Entity object</param>
        public void AddEntity(SceneEntity entity)
        {
            this.entities.Add(entity);
        }

        /// <summary>
        /// Add a point light to the scene that should be computed.
        /// </summary>
        /// <param name="light">Light structure</param>
        public void AddPointLight(PointLight light)
        {
            this.lights.Add(light);
        }

        /// <summary>
        /// Render the scene to an output image. This is where the bulk
        /// of your ray tracing logic should go... though you may wish to
        /// break it down into multiple functions as it gets more complex!
        /// </summary>
        /// <param name="outputImage">Image to store render output</param>
        public void Render(Image outputImage)
        {
            Vector3 camera = options.CameraPosition;
            double gridSizeX = 1.0d/outputImage.Width;
            double gridSizeY = 1.0d/outputImage.Height;

            for (int i=0; i < outputImage.Width; i++)
            for (int j=0; j < outputImage.Height; j++)
            {   
                Ray ray = new Ray(camera, NormalisedWorldCoordinate((i + 0.5) * gridSizeX, (j + 0.5) * gridSizeY, 1.0d, outputImage));

                foreach(var entity in this.entities)
                {
                    RayHit hit = entity.Intersect(ray);
                    Color color = new Color(0.0f, 0.0f, 0.0f);

                    if (hit != null)
                    {
                        Vector3 hitNormal = hit.Normal.Normalized();
                        Boolean directLight = true;
                        foreach (PointLight pointLight in this.lights)
                        {
                            Vector3 hit2Light = (pointLight.Position - hit.Position).Normalized();
                            Ray lightRay = new Ray(pointLight.Position, -hit2Light);

                            foreach(var otherEntity in this.entities) 
                            {
                                if (otherEntity.Equals(entity)) continue;

                                RayHit hit2 = otherEntity.Intersect(lightRay); 

                                if (hit2 != null) 
                                {
                                    Vector3 cmphit1 = hit.Position - pointLight.Position;
                                    Vector3 cmphit2 = hit2.Position - pointLight.Position;
                                    
                                    if (cmphit1.Dot(cmphit2) < cmphit1.Dot(cmphit1))
                                    {
                                        directLight = false;
                                        break;
                                    }
                                }
                            }

                            if (!directLight) break;

                            if (hitNormal.Dot(hit2Light) > 0 ) 
                            {
                                color += (entity.Material.Color * pointLight.Color) * hitNormal.Dot(hit2Light);
                            }
                        }
                        outputImage.SetPixel(i, j, color);
                    }
                }
            }
        }

        private Vector3 NormalisedWorldCoordinate(double x, double y, double z, Image outputImage)
        {
            // Defining plane as it appears when embedded in the scene
            double fieldOfView = 60.0d;
            double aspectRatio = outputImage.Width / outputImage.Height;
            double Deg2Rad = Math.PI/180.0d;

            // 1.0d not necessary, but it represent the distance from the camera
            double fovLength = 2.0d * Math.Tan(fieldOfView*Deg2Rad / 2) * 1.0d;
            double imagePlaneHeight = fovLength;
            double imagePlaneWidth = fovLength * aspectRatio;

            double cx = (x - 0.5d) * imagePlaneWidth;
            double cy = (0.5d - y) * imagePlaneHeight;
            double cz = z;

            return new Vector3(cx, cy, cz);
        }
    }
}
