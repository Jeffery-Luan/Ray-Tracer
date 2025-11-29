[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/oMRiv2DB)
# COMP30019 - Project 1 - Ray Tracer

This is your README.md... you should write anything relevant to your
implementation here.

Please ensure your student details are specified below (*exactly* as on UniMelb
records):

**Name:** Yudong Luan \
**Student Number:** 1362030 \
**Username:** YUDLUAN \
**Email:** yudluan@student.unimelb.edu.au

## Completed stages

Tick the stages bellow that you have completed so we know what to mark (by
editing README.md). **At most 3** add-ons can be chosen for marking of stage three. If you complete more than this, pick your best one(s) to be marked, otherwise we will pick at random!

<!---
Tip: To tick, place an x between the square brackes [ ], like so: [x]
-->

##### Stage 1

- [x] Stage 1.1 - Familiarise yourself with the template
- [x] Stage 1.2 - Implement vector mathematics
- [x] Stage 1.3 - Fire a ray for each pixel
- [x] Stage 1.4 - Calculate ray-entity intersections
- [x] Stage 1.5 - Output primitives as solid colours

##### Stage 2

- [x] Stage 2.1 - Illumination
- [x] Stage 2.2 - Shadow rays
- [x] Stage 2.3 - Reflection rays
- [x] Stage 2.4 - Refraction rays
- [x] Stage 2.5 - The Whitted Illumination Model

##### Stage 3

- [x] Stage 3.1 - Advanced features
- [x] Stage 3.2 - Advanced add-ons
  - [x] A.1 - Anti-aliasing
  - [ ] A.2 - Soft shadows
  - [x] A.3 - Depth of field blur
  - [ ] A.4 - Motion blur
  - [ ] B.1 - Color texture mapping
  - [ ] B.2 - Bump or normal mapping
  - [ ] B.3 - Procedural textures
  - [x] C.1 - Simple animation
  - [ ] C.2 - Keyframe animation
  - [ ] C.3 - Camera animation

*Please summarise your approach(es) to stage 3 here.*
OBJ & Camera (3.1)
Custom OBJ loader supporting v, optional vn, and triangular f (v, v//n, v/vt/n).
Preserve original winding so face normals point outward.
Build world-space triangles; compute AABB (slab early-out) + a bounding sphere; choose the nearest hit via |P − O|.
Camera uses a Transform (position + quaternion) to rotate local ray directions into world space.

Whitted pipeline (foundation for 3.1/3.2)
Hard shadows: cast from P + N·EPS, skip the hit entity itself, and only count blockers with EPS < t < distToLight − EPS.
Reflection & refraction with bounded recursion (depth ≤ 5).
Fresnel (Schlick) only for transmissive materials; detect inside/outside by the sign of D·N, swap IORs when inside, handle TIR.
Refraction rays are biased by ±N (enter/exit) rather than along T to avoid self-intersection.

A1 — Anti-aliasing
N×N stratified jitter per pixel (N = AAMultiplier), average samples.

A3 — Depth of Field (thin-lens)
Compute each pixel’s focus point on the focal plane z = FocalLength (camera-local).
Sample a uniform disk aperture (r = R√U, θ = 2πV) for the ray origin, then aim at the same focus point.
Falls back to pinhole when ApertureRadius = 0. AA and DOF share the same sampling loop.

C1 — Simple animation
For ObjModels, apply per-frame translation + rotation about the current model center, then rebuild triangles and update AABB.
Applied once per frame before rendering.

Numerics & limitations
Unified EPS = 1e-4; normalize all directions used in dot products.
Triangles are flat-shaded (no interpolated vn yet).
Soft shadows / textures / normal mapping not implemented.
DOF introduces Monte-Carlo noise at low SPP—raise -x to reduce grain.

## Final scene render

Be sure to replace ```/images/final_scene.png``` with your final render so it
shows up here.

![My final render](images/final_scene.png)

This render took **0** minutes and **20** seconds on my PC.

I used the following command to render the image exactly as shown:

```
dotnet run -- -f tests/final_scene.txt -o images/final_scene.png -x 1 -r 0
```

## Sample outputs

We have provided you with some sample tests located at ```/tests/*```. So you
have some point of comparison, here are the outputs our ray tracer solution
produces for given command line inputs (for the first two stages, left and right
respectively):

###### Sample 1

```
dotnet run -- -f tests/sample_scene_1.txt -o images/sample_scene_1.png
```

<p float="left">
  <img src="images/sample_scene_1_s1.png" />
  <img src="images/sample_scene_1_s2.png" /> 
</p>

###### Sample 2

```
dotnet run -- -f tests/sample_scene_2.txt -o images/sample_scene_2.png
```

<p float="left">
  <img src="images/sample_scene_2_s1.png" />
  <img src="images/sample_scene_2_s2.png" /> 
</p>

## References

*You must list any references you used - add them here!*
Lecture notes and project spec for COMP30019

