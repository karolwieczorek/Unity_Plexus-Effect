using System.Collections.Generic;
using UnityEngine;

namespace Game {
    public class PlexusEffect : MonoBehaviour
    {
        [SerializeField] PlexusEffectData data;

        Transform targetTransform;

        new ParticleSystem particleSystem;
        ParticleSystem.Particle[] particles;

        ParticleSystem.MainModule particleSystemMainModule;

        List<LineRenderer> lineRenderers = new List<LineRenderer>();

        ParticleSystemSimulationSpace simulationSpace;
        uint connections = 0;

        private void Start()
        {
            particleSystem = GetComponent<ParticleSystem>();
            particleSystemMainModule = particleSystem.main;
            targetTransform = transform;
        }

        private void LateUpdate()
        {
            int maxParticles = particleSystemMainModule.maxParticles;

            if (particles == null || particles.Length < maxParticles)
            {
                particles = new ParticleSystem.Particle[maxParticles];
            }

            particleSystem.GetParticles(particles);
            int particlesCount = particleSystem.particleCount;

            float maxDistanceSqr = data.maxDistance * data.maxDistance;

            simulationSpace = particleSystemMainModule.simulationSpace;
            SimulationSpaceAdjust(simulationSpace);

            int lrIndex = 0;

            for (int i = 0; i < particlesCount; i++)
            {

                if (lrIndex >= data.maxLineRenderers)
                    break;
                connections = 0;
                ParticleSystem.Particle p1 = particles[i];

                for (int j = i + 1; j < particlesCount; j++)
                {
                    if (connections >= data.maxConnections || lrIndex >= data.maxLineRenderers)
                        break;

                    ParticleSystem.Particle p2 = particles[j];
                    lrIndex = SetupLineRenderer(maxDistanceSqr, lrIndex, p1, p2);
                }
            }

            for (int i = lrIndex; i < lineRenderers.Count; i++)
            {
                lineRenderers[i].enabled = false;
            }
        }

        private int SetupLineRenderer(float maxDistanceSqr, int lrIndex, ParticleSystem.Particle p1, ParticleSystem.Particle p2)
        {
            float distanceSqr = Vector3.SqrMagnitude(p1.position - p2.position);
            int lineRendererCount = lineRenderers.Count;

            if (distanceSqr <= maxDistanceSqr)
            {
                LineRenderer lr;

                if (lrIndex == lineRendererCount)
                {
                    lr = Instantiate(data.lineRendererTemplate, targetTransform, false);
                    lineRenderers.Add(lr);

                    lineRendererCount++;
                }

                lr = lineRenderers[lrIndex];

                lr.enabled = true;
                lr.material = data.lineRendererMaterial;
                lr.startWidth = lr.endWidth = data.width;
                lr.useWorldSpace = simulationSpace == ParticleSystemSimulationSpace.World;
                lr.SetPosition(0, p1.position);
                lr.SetPosition(1, p2.position);
                lr.colorGradient = ParticleGradientColor(p1, p2);

                lrIndex++;
                connections++;
            }
            return lrIndex;
        }

        private Gradient ParticleGradientColor(ParticleSystem.Particle p1, ParticleSystem.Particle p2)
        {
            Gradient gradient = new Gradient();
            float alpha = 1f;
            Color32 p1_color = p1.GetCurrentColor(particleSystem);
            Color32 p2_color = p2.GetCurrentColor(particleSystem);
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(p1_color, 0.0f), new GradientColorKey(p2_color, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
                );
            return gradient;
        }

        private void SimulationSpaceAdjust(ParticleSystemSimulationSpace simulationSpace)
        {
            switch (simulationSpace)
            {
                case ParticleSystemSimulationSpace.Local:
                    {
                        targetTransform = transform;
                        break;
                    }
                case ParticleSystemSimulationSpace.Custom:
                    {
                        targetTransform = particleSystemMainModule.customSimulationSpace;
                        break;
                    }
                case ParticleSystemSimulationSpace.World:
                    {
                        targetTransform = transform;
                        break;
                    }
                default:
                    {
                        throw new System.NotSupportedException(string.Format("Unsupported simulation space '{0}'.",
                            System.Enum.GetName(typeof(ParticleSystemSimulationSpace), particleSystemMainModule.simulationSpace)));
                    }
            }
        }
    }
}