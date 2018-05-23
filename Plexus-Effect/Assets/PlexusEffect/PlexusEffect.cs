using System.Collections.Generic;
using UnityEngine;

namespace Game {
    public class PlexusEffect : MonoBehaviour
    {
        [SerializeField] float maxDistance = 1.0f;

        Transform targetTransform;

        new ParticleSystem particleSystem;
        ParticleSystem.Particle[] particles;

        ParticleSystem.MainModule particleSystemMainModule;

        [SerializeField]  LineRenderer lineRendererTemplate;
        List<LineRenderer> lineRenderers = new List<LineRenderer>();

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

            float maxDistanceSqr = maxDistance * maxDistance;

            ParticleSystemSimulationSpace simulationSpace = particleSystemMainModule.simulationSpace;
            SimulationSpaceAdjust(simulationSpace);

            int lrIndex = 0;
            int lineRendererCount = lineRenderers.Count;

            for (int i = 0; i < particlesCount; i++)
            {
                Vector3 p1_position = particles[i].position;
                for (int j = i + 1; j < particlesCount; j++)
                {
                    Vector3 p2_position = particles[j].position;
                    float distanceSqr = Vector3.SqrMagnitude(p1_position - p2_position);

                    if (distanceSqr <= maxDistanceSqr)
                    {
                        LineRenderer lr;

                        if (lrIndex == lineRendererCount)
                        {
                            lr = Instantiate(lineRendererTemplate, targetTransform, false);
                            lineRenderers.Add(lr);

                            lineRendererCount++;
                        }

                        lr = lineRenderers[lrIndex];

                        lr.enabled = true;
                        lr.useWorldSpace = simulationSpace == ParticleSystemSimulationSpace.World;
                        lr.SetPosition(0, p1_position);
                        lr.SetPosition(1, p2_position);

                        lrIndex++;
                    }
                }
            }

            for (int i = lrIndex; i < lineRendererCount; i++)
            {
                lineRenderers[i].enabled = false;
            }
        }

        private void SimulationSpaceAdjust(ParticleSystemSimulationSpace simulationSpace)
        {
            switch (simulationSpace)
            {
                case ParticleSystemSimulationSpace.Local:
                    {
                        targetTransform = transform;
                        lineRendererTemplate.useWorldSpace = false;
                        break;
                    }
                case ParticleSystemSimulationSpace.Custom:
                    {
                        targetTransform = particleSystemMainModule.customSimulationSpace;
                        lineRendererTemplate.useWorldSpace = false;
                        break;
                    }
                case ParticleSystemSimulationSpace.World:
                    {
                        targetTransform = transform;
                        lineRendererTemplate.useWorldSpace = true;
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