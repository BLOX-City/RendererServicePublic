public static class OklabConverter
{
    // Define your conversion matrices here as plain 2D arrays or similar.
    // For 3x3, a 2D array is natural and clear.

    private static readonly float[,] LinearRgbToLmsMatrix = new float[,]
    {
        { 0.8190222163f, 0.3619062806f, -0.1288737826f },
        { 0.0329836672f, 0.9292864016f, 0.0361446682f },
        { 0.0170370813f, 0.0412610131f, 0.9405616908f }
    };

    private static readonly float[,] LmsToOklabMatrix = new float[,]
    {
        { 0.2104542553f, 0.7936177850f, -0.0040720468f },
        { 1.9779984951f, -2.4285922050f, 0.4505937109f },
        { 0.0259083636f, 0.7827717466f, -0.8086757041f }
    };

    public static (float L, float a, float b) RgbToOklab(float r_sRGB, float g_sRGB, float b_sRGB)
    {
        // 1. sRGB to Linear RGB
        float r_linear = SrgbToLinear(r_sRGB);
        float g_linear = SrgbToLinear(g_sRGB);
        float b_linear = SrgbToLinear(b_sRGB);

        // 2. Linear RGB to LMS (Matrix multiplication: LMS = LinearRgbToLmsMatrix * [r_linear, g_linear, b_linear]^T)
        // This is now explicitly a scalar matrix-vector multiplication.
        float l_lms = LinearRgbToLmsMatrix[0, 0] * r_linear +
                      LinearRgbToLmsMatrix[0, 1] * g_linear +
                      LinearRgbToLmsMatrix[0, 2] * b_linear;

        float m_lms = LinearRgbToLmsMatrix[1, 0] * r_linear +
                      LinearRgbToLmsMatrix[1, 1] * g_linear +
                      LinearRgbToLmsMatrix[1, 2] * b_linear;

        float s_lms = LinearRgbToLmsMatrix[2, 0] * r_linear +
                      LinearRgbToLmsMatrix[2, 1] * g_linear +
                      LinearRgbToLmsMatrix[2, 2] * b_linear;

        // 3. Apply Cube Root
        float l_ok = MathF.Cbrt(l_lms);
        float m_ok = MathF.Cbrt(m_lms);
        float s_ok = MathF.Cbrt(s_lms);

        // 4. LMS (cube-rooted) to Oklab (Matrix multiplication: Oklab = LmsToOklabMatrix * [l_ok, m_ok, s_ok]^T)
        float L_oklab = LmsToOklabMatrix[0, 0] * l_ok +
                        LmsToOklabMatrix[0, 1] * m_ok +
                        LmsToOklabMatrix[0, 2] * s_ok;

        float a_oklab = LmsToOklabMatrix[1, 0] * l_ok +
                        LmsToOklabMatrix[1, 1] * m_ok +
                        LmsToOklabMatrix[1, 2] * s_ok;

        float b_oklab = LmsToOklabMatrix[2, 0] * l_ok +
                        LmsToOklabMatrix[2, 1] * m_ok +
                        LmsToOklabMatrix[2, 2] * s_ok;

        return (L_oklab, a_oklab, b_oklab);
    }

    private static float SrgbToLinear(float c_sRGB)
    {
        if (c_sRGB <= 0.04045f)
        {
            return c_sRGB / 12.92f;
        }
        else
        {
            return MathF.Pow((c_sRGB + 0.055f) / 1.055f, 2.4f);
        }
    }

    /// <summary>
    /// Scales an Oklab 'a' or 'b' component to reduce bias towards the center
    /// for Euclidean difference comparisons.
    /// </summary>
    /// <param name="x">The 'a' or 'b' component.</param>
    /// <param name="chroma">The chroma (sqrt(a^2 + b^2)) of the color.</param>
    /// <returns>The scaled component value.</returns>
    private static float ScaleComponentForDiff(float x, float chroma)
    {
        // Using 1e-6f for float literal
        return x / (1e-6f + MathF.Pow(chroma, 0.5f));
    }

    /// <summary>
    /// Converts discrete bit configurations (ll, aaa, bbb) back into Oklab L, a, b values.
    /// </summary>
    /// <param name="ll">2-bit L value (0-3).</param>
    /// <param name="aaa">3-bit a value (0-7).</param>
    /// <param name="bbb">3-bit b value (0-7).</param>
    /// <returns>A tuple containing the corresponding Oklab L, a, b values.</returns>
    public static (float L, float a, float b) BitsToLab(int ll, int aaa, int bbb)
    {
        // Convert binary literals to integer constants in C#
        const int L_MAX_BITS_VAL = 3;   // 0b11
        const int A_MAX_BITS_VAL = 8;   // 0b1000
        const int B_MAX_BITS_VAL = 8;   // 0b1000

        float L_val = (ll / (float)L_MAX_BITS_VAL) * 0.6f + 0.2f;
        float a_val = (aaa / (float)A_MAX_BITS_VAL) * 0.7f - 0.35f;
        // Note: The JavaScript `(bbb + 1) / 0b1000` for 'b' means the bbb range effectively shifts.
        // If bbb is 0-7, (bbb+1) is 1-8. So it uses the full 8 steps.
        float b_val = ((bbb + 1) / (float)B_MAX_BITS_VAL) * 0.7f - 0.35f;

        return (L_val, a_val, b_val);
    }

    /// <summary>
    /// Calculates the hypotenuse of two floats (equivalent to Math.Hypot for doubles).
    /// </summary>
    /// <param name="x">The first value.</param>
    /// <param name="y">The second value.</param>
    /// <returns>The hypotenuse, sqrt(x*x + y*y).</returns>
    private static float CalculateHypot(float x, float y)
    {
        return MathF.Sqrt(x * x + y * y);
    }    

    private static float CalculateHypot3D(float x, float y, float z)
    {
        return MathF.Sqrt(x * x + y * y + z * z);
    }    

    /// <summary>
    /// Finds the bit configuration (ll, aaa, bbb) that produces an Oklab color
    /// closest to the target Oklab color.
    /// </summary>
    /// <param name="targetL">Target Oklab L component.</param>
    /// <param name="targetA">Target Oklab a component.</param>
    /// <param name="targetB">Target Oklab b component.</param>
    /// <returns>A tuple containing the best ll, aaa, and bbb bit configurations.</returns>
    public static (int ll, int aaa, int bbb) FindOklabBits(float targetL, float targetA, float targetB)
    {
        float targetChroma = CalculateHypot(targetA, targetB);
        float scaledTargetA = ScaleComponentForDiff(targetA, targetChroma);
        float scaledTargetB = ScaleComponentForDiff(targetB, targetChroma);

        int bestLl = 0;
        int bestAaa = 0;
        int bestBbb = 0;
        float bestDifference = float.PositiveInfinity; // Equivalent to Infinity in JS

        // Loop through all possible bit combinations
        // 0b11 is 3 (2 bits, 0-3 inclusive)
        // 0b111 is 7 (3 bits, 0-7 inclusive)
        for (int lli = 0; lli <= 3; lli++)
        {
            for (int aaai = 0; aaai <= 7; aaai++)
            {
                for (int bbbi = 0; bbbi <= 7; bbbi++)
                {
                    (float L, float a, float b) currentOklab = BitsToLab(lli, aaai, bbbi);

                    float chroma = CalculateHypot(currentOklab.a, currentOklab.b);
                    float scaledA = ScaleComponentForDiff(currentOklab.a, chroma);
                    float scaledB = ScaleComponentForDiff(currentOklab.b, chroma);

                    float difference = CalculateHypot3D(
                        currentOklab.L - targetL,
                        scaledA - scaledTargetA,
                        scaledB - scaledTargetB
                    );

                    if (difference < bestDifference)
                    {
                        bestDifference = difference;
                        bestLl = lli;
                        bestAaa = aaai;
                        bestBbb = bbbi;
                    }
                }
            }
        }

        return (ll: bestLl, aaa: bestAaa, bbb: bestBbb);
    }    
}