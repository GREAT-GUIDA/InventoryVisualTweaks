using GuidaSharedCode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuidaSharedCode {
    public class TwistCircleParticle : Particle {
        public float size = 1.4f;
        public float strength = 0.1f;
        public int time = 30;
        public int timer = 0;
        public float image_alpha = 0;
        public float image_scale = 1;
        public bool inverse = false;
        public override void SetDefaults() {
            drawLayer = ParticleLayer.Twist;
            base.SetDefaults();
        }
        public override void AI() {
            timer += 1;
            float rate = (float)timer / time;
            if (inverse) rate = 1f - rate;
            image_alpha = (1f - rate) * strength * 6f;
            image_scale = rate * size * 1.2f;

            if (timer >= time) Kill();
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            return false;
        }
    }
}
