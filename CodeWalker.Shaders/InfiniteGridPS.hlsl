cbuffer CameraBuffer : register(b0)
{
    float4x4 InvViewProj; // Inverse of (View * Projection)
    float4 CameraDirection;
    float4 CameraPosition;
    float Viewport_x;
    float Viewport_y;
    float Viewport_width;
    float Viewport_height;
    float Viewport_minZ;
    float Viewport_maxZ;
    float GridScale; // e.g. 1.0 = 1 unit per small cell
    float MajorGridDiv; // e.g. 10.0 = every 10 small cells is a major line
    float FadeDistance; // how far before grid fades out
    float LineThickness; // 0.02 ~ 0.05 looks good
};

struct PSInput
{
    float4 Position : SV_POSITION;
    float2 ScreenUV : TEXCOORD0;
};

#define vec3 float3
#define vec2 float2

float pristineGrid(in vec2 uv, in vec2 ddx, in vec2 ddy, vec2 lineWidth)
{
    vec2 uvDeriv = vec2(length(vec2(ddx.x, ddy.x)), length(vec2(ddx.y, ddy.y)));
    bool2 invertLine = bool2(lineWidth.x > 0.5, lineWidth.y > 0.5);
    vec2 targetWidth = vec2(
        invertLine.x ? 1.0 - lineWidth.x : lineWidth.x,
        invertLine.y ? 1.0 - lineWidth.y : lineWidth.y
    );
    vec2 drawWidth = clamp(targetWidth, uvDeriv, vec2(0.5, 0.5));
    vec2 lineAA = uvDeriv * 1.5;
    vec2 gridUV = abs(frac(uv) * 2.0 - 1.0);
    gridUV.x = invertLine.x ? gridUV.x : 1.0 - gridUV.x;
    gridUV.y = invertLine.y ? gridUV.y : 1.0 - gridUV.y;
    vec2 grid2 = smoothstep(drawWidth + lineAA, drawWidth - lineAA, gridUV);

    grid2 *= clamp(targetWidth / drawWidth, 0.0, 1.0);
    grid2 = lerp(grid2, targetWidth, clamp(uvDeriv * 2.0 - 1.0, 0.0, 1.0));
    grid2.x = invertLine.x ? 1.0 - grid2.x : grid2.x;
    grid2.y = invertLine.y ? 1.0 - grid2.y : grid2.y;
    return lerp(grid2.x, 1.0, grid2.y);
}

void calcCamera(out float3 ro, out float3 ta)
{
    float an = 0.1 * sin(0.1);
    ro = float3(5.0 * cos(an), 0.5, 5.0 * sin(an));
    ro = CameraPosition.xyz;
    ta = (CameraPosition + CameraDirection).xyz;
    // ro = CameraDirection;
    // ta = CameraPosition;
    // ta = vec3(0.0, 1.0, 0.0);
}

void calcRayForPixel(in float2 pix, out float3 resRo, out float3 resRd)
{
    float2 iResolution = float2(Viewport_width, Viewport_height);
    float2 p = (2.0 * pix - iResolution.xy) / iResolution.y;

    // camera movement	
    float3 ro, ta;
    calcCamera(ro, ta);
    // camera matrix
    float3 ww = normalize(ta - ro);
    float3 uu = normalize(cross(ww, float3(0.0, 0.0, 1.0)));
    float3 vv = normalize(cross(uu, ww));
    // create view ray
    float3 rd = normalize(p.x * uu + p.y * vv + 2.0 * ww);

    resRo = ro;
    resRd = rd;
}

float intersect(vec3 ro, vec3 rd, out vec3 pos, out vec3 nor, out float occ, out int matid)
{
    // raytrace
    float tmin = 10000.0;
    nor = vec3(0, 0, 0);
    occ = 1.0;
    pos = vec3(0, 0, 0);
    matid = -1;

    // raytrace-plane
    float h = (0.0001 - ro.z) / rd.z;
    if (h > 0.0)
    {
        tmin = h;
        nor = vec3(0.0, 0.0, 1.0);
        pos = ro + h * rd;
        matid = 0;
    }

    return tmin;
}

vec2 texCoords(in vec3 pos, int mid)
{
    vec2 matuv;
    if (mid == 0)
    {
        matuv = pos.xy;
        
        // float dist = distance(CameraPosition.xyz, pos);
        // float gridLOD = floor(log10(dist));
        // float gridSize = pow(10.0, gridLOD);
        // matuv = pos.xy / gridSize;
    }
    return 8.0 * matuv;
}

#define N 50
#define PRISTINEGRID

float4 main(PSInput input) : SV_TARGET
{
    // #ifdef PRISTINEGRID
    float2 iResolution = float2(Viewport_width, Viewport_height);
    float2 fragCoord = float2(iResolution.x * input.ScreenUV.x, iResolution.y * (1 - input.ScreenUV.y));

    float2 p = (-iResolution.xy + 2.0 * fragCoord) / iResolution.y;

    // return float4(p, 0, 1);
    float3 ro, rd, ddx_ro, ddx_rd, ddy_ro, ddy_rd;
    calcRayForPixel(fragCoord + float2(0.0, 0.0), ro, rd);
    calcRayForPixel(fragCoord + float2(1.0, 0.0), ddx_ro, ddx_rd);
    calcRayForPixel(fragCoord + float2(0.0, 1.0), ddy_ro, ddy_rd);

    float3 pos, nor;
    float occ;
    int mid;
    float t = intersect(ro, rd, pos, nor, occ, mid);
    float4 col = float4(0, 0, 0, 1);
    if (mid != -1)
    {
        // computer ray differentials
        vec3 ddx_pos = ddx_ro - ddx_rd * dot(ddx_ro - pos, nor) / dot(ddx_rd, nor);
        vec3 ddy_pos = ddy_ro - ddy_rd * dot(ddy_ro - pos, nor) / dot(ddy_rd, nor);

        // calc texture sampling footprint		
        vec2 uv = texCoords(pos, mid);
        vec2 ddx_uv = texCoords(ddx_pos, mid) - uv;
        vec2 ddy_uv = texCoords(ddy_pos, mid) - uv;

        float lineWidth = 1.0 / N;
        vec3 mate = pristineGrid(uv, ddx_uv, ddy_uv, float2(lineWidth, lineWidth));

        col.rgb = lerp(mate, vec3(0, 0, 0), 1.0 - exp(-0.00001 * t * t));
        col.rgb = pow(col.rgb, vec3(0.4545, 0.4545, 0.4545));
    }
    // return float4(p, 0, 1);
    return col;
    /*
// #else
    // Reconstruct world position on Y=0 plane from screen pixel
    float4 ndc = float4(input.ScreenUV.x * 2.0 - 1.0, input.ScreenUV.y * 2.0 - 1.0, 0.1, 1.0); // near plane
    float4 worldPosH = mul(ndc, InvViewProj);
    float3 worldPos = worldPosH.xyz / worldPosH.w;

    //return float4(CameraPosition.xy / 50, 0, 1);
    //return float4(input.ScreenUV, 0, 1);
    //return float4(ndc.x, ndc.y, 0, 1);
    //return float4(worldPos.x, 0 / 100, 0, 1);

    // Ray from camera to world point on the plane
    float3 rayOrigin = CameraPosition;
    float3 rayDir = normalize(worldPos - rayOrigin);
    // float3 hitPos1;
    // if (RayMarch(CameraPosition, rayDir, hitPos1))
    // {
    //     return float4(hitPos1, 1.0f);
    // }

    return float4(0, 0, 0, 1);
    // Intersect with Y=0 plane (horizontal ground)
    if (abs(rayDir.y) < 0.0001)
        discard; // parallel to plane

    float t = -rayOrigin.y / rayDir.y;
    if (t < 0.0)
        discard; // behind camera

    float3 hitPos = rayOrigin + rayDir * t;

    // Grid calculation
    float2 gridPos = hitPos.xz / GridScale;

    // Minor and major lines with anti-aliasing
    float2 minor = abs(frac(gridPos) - 0.5);
    float2 major = abs(frac(gridPos / MajorGridDiv) - 0.5);

    float2 deriv = fwidth(gridPos); // anti-aliasing
    float minorLine = smoothstep(0.0, deriv.x * LineThickness, minor.x) *
        smoothstep(0.0, deriv.y * LineThickness, minor.y);

    float majorLine = smoothstep(0.0, deriv.x * LineThickness * 2.0, major.x) *
        smoothstep(0.0, deriv.y * LineThickness * 2.0, major.y);

    // Combine lines (major lines are brighter)
    float grid = 1.0 - min(minorLine, majorLine * 0.6 + 0.4);

    // Fade with distance
    float dist = length(hitPos - rayOrigin);
    float alpha = saturate(1.0 - dist / FadeDistance);

    float3 gridColor = lerp(float3(0.6, 0.6, 0.6), float3(0.9, 0.9, 0.9), majorLine);

    //return float4(input.ScreenUV, 0, 1);
    return float4(gridColor, grid * alpha * 0.8); // adjust alpha for desired opacity
    */
    // #endif
}
