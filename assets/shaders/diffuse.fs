#version 330
in vec3 normal;
in vec3 objPosition;
in vec3 perVertexLighting;
in vec2 texCoordData;
in vec3 vertColor;
out vec4 renderResult;
uniform sampler2D tex;
uniform int isTextureEnabled;
uniform int isVertexColorEnabled;
uniform int hasNoLighting;
uniform int color;



float luma(vec3 color) {
    return dot(color, vec3(0.2126, 0.7152, 0.0722));
}

const float bayer8x8[64] = float[64](
     0.0/64.0, 48.0/64.0, 12.0/64.0, 60.0/64.0,  3.0/64.0, 51.0/64.0, 15.0/64.0, 63.0/64.0,
    32.0/64.0, 16.0/64.0, 44.0/64.0, 28.0/64.0, 35.0/64.0, 19.0/64.0, 47.0/64.0, 31.0/64.0,
     8.0/64.0, 56.0/64.0,  4.0/64.0, 52.0/64.0, 11.0/64.0, 59.0/64.0,  7.0/64.0, 55.0/64.0,
    40.0/64.0, 24.0/64.0, 36.0/64.0, 20.0/64.0, 43.0/64.0, 27.0/64.0, 39.0/64.0, 23.0/64.0,
     2.0/64.0, 50.0/64.0, 14.0/64.0, 62.0/64.0,  1.0/64.0, 49.0/64.0, 13.0/64.0, 61.0/64.0,
    34.0/64.0, 18.0/64.0, 46.0/64.0, 30.0/64.0, 33.0/64.0, 17.0/64.0, 45.0/64.0, 29.0/64.0,
    10.0/64.0, 58.0/64.0,  6.0/64.0, 54.0/64.0,  9.0/64.0, 57.0/64.0,  5.0/64.0, 53.0/64.0,
    42.0/64.0, 26.0/64.0, 38.0/64.0, 22.0/64.0, 41.0/64.0, 25.0/64.0, 37.0/64.0, 21.0/64.0
);

vec4 dither(vec2 fragCoord, vec4 color) {
    ivec2 pos = ivec2(floor(fragCoord));
    int x = pos.x % 8;
    int y = pos.y % 8;
    int index = x + y * 8;
    float threshold = bayer8x8[index];

    float levels = 105.0; // 10-bit quantization steps
    vec4 scaled = floor(color * levels + threshold);
    return scaled / levels;
}
void main()
{
    vec4 textureColor = vec4(vec3(1.0), 0.0);
    
    if(isTextureEnabled == 1) {
        textureColor = texture(tex, vec2(texCoordData.x, -texCoordData.y));

        if(hasNoLighting == 1) {
            /*textureColor.a = 1.0;
            textureColor.r = 1.0;
            textureColor.g = 1.0;
            textureColor.b = 1.0;*/

            renderResult = textureColor;
        }
    }
    else {
        textureColor.a = 1.0;
    }
    vec3 fragmentColor =  perVertexLighting;

    if (hasNoLighting == 1) {
        fragmentColor = vec3(1, 1, 1);
    }

    if (textureColor.x <= 0.2 && textureColor.y <= 0.2 && textureColor.z <= 0.2) {
        // 25.0 originally but llvmpipe blows colors
        textureColor += vec4(fragmentColor / 145.0, 0.0);
    }
    

    uint unsignedColor = uint(color);
    vec4 objectColor = vec4(
    float((unsignedColor >> 16u) & 0xFFu) / 255.0,
    float((unsignedColor >> 8u) & 0xFFu) / 255.0,
    float((unsignedColor >> 0u) & 0xFFu) / 255.0,
    float((unsignedColor >> 24u) & 0xFFu) / 255.0);

    if (isVertexColorEnabled == 1) {
        objectColor *= vec4(vertColor, 1.0);
    }

    // originally * fragmentColor
    renderResult = vec4(objectColor.xyz * max(vec3(0.5), (hasNoLighting == 1 ? fragmentColor : fragmentColor / 15.0)), textureColor.a) * dither(gl_FragCoord.xy, textureColor); //  * vec4(fragmentColor, 1.0)
    
}