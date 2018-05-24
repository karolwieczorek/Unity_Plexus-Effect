using UnityEngine;
using System.Collections.Generic;

namespace Game
{    
    [CreateAssetMenu(fileName = "PlexusEffectData", menuName = "Custom/PlexusEffectData", order = 1)]
    public class PlexusEffectData : ScriptableObject
    {
        public float maxDistance = 1.0f;
        public float width = 0.1f;
        public uint maxConnections = 5;
        public uint maxLineRenderers = 100;
        public LineRenderer lineRendererTemplate;
        public Material lineRendererMaterial;
    }
}