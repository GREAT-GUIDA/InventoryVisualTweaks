using System;
using Microsoft.Xna.Framework;

namespace GuidaSharedCode {
    public static class TrailSplineHelper {
        public static int BuildSmoothPoints(
            Vector2[] sourcePoints,
            int sourceCount,
            int samplesPerSegment,
            float bezierTension,
            Vector2[] smoothPoints) {
            if (sourceCount <= 0)
                return 0;

            if (sourceCount == 1) {
                smoothPoints[0] = sourcePoints[0];
                return 1;
            }

            if (samplesPerSegment < 1)
                samplesPerSegment = 1;

            return BuildBezier(sourcePoints, sourceCount, samplesPerSegment, bezierTension, smoothPoints);
        }

        private static int BuildBezier(
            Vector2[] sourcePoints,
            int sourceCount,
            int samplesPerSegment,
            float tension,
            Vector2[] smoothPoints) {
            return BuildSegments(sourcePoints, sourceCount, samplesPerSegment, smoothPoints, (segment, t) => {
                Vector2 p0 = sourcePoints[segment];
                Vector2 p3 = sourcePoints[segment + 1];
                Vector2 tangent0 = GetTrailTangent(sourcePoints, sourceCount, segment);
                Vector2 tangent1 = GetTrailTangent(sourcePoints, sourceCount, segment + 1);

                float chord = Vector2.Distance(p0, p3);
                if (chord < 0.001f)
                    return p0;

                float handleLength = chord * tension;
                Vector2 p1 = p0 + tangent0 * handleLength;
                Vector2 p2 = p3 - tangent1 * handleLength;
                return EvaluateBezier(p0, p1, p2, p3, t);
            });
        }

        private static int BuildSegments(
            Vector2[] sourcePoints,
            int sourceCount,
            int samplesPerSegment,
            Vector2[] smoothPoints,
            Func<int, float, Vector2> evaluateSegment) {
            int smoothCount = 0;
            int segmentCount = sourceCount - 1;

            for (int segment = 0; segment < segmentCount; segment++) {
                int stepCount = segment == segmentCount - 1 ? samplesPerSegment + 1 : samplesPerSegment;
                for (int step = 0; step < stepCount; step++) {
                    if (segment > 0 && step == 0)
                        continue;

                    float t = step / (float)samplesPerSegment;
                    smoothPoints[smoothCount++] = evaluateSegment(segment, t);
                }
            }

            return smoothCount;
        }

        private static Vector2 EvaluateBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t) {
            float u = 1f - t;
            float u2 = u * u;
            float t2 = t * t;

            return u2 * u * p0
                + 3f * u2 * t * p1
                + 3f * u * t2 * p2
                + t2 * t * p3;
        }

        private static Vector2 GetTrailTangent(Vector2[] points, int count, int index) {
            Vector2 tangent;

            if (index <= 0)
                tangent = points[0] - points[1];
            else if (index >= count - 1)
                tangent = points[count - 2] - points[count - 1];
            else
                tangent = (points[index] - points[index + 1]) + (points[index - 1] - points[index]);

            if (tangent.LengthSquared() < 0.0001f)
                return Vector2.UnitY;

            return Vector2.Normalize(tangent);
        }
    }
}
