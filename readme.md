# Boids in Unity DOTS

![](https://github.com/d-bucur/demos/raw/master/boids.gif)


## Description
Playing around with the DOTS preview package + physics to create a massively parallel simulation of boids

## Controls
- **Right** click to rotate camera
- **WASD** to move camera
- **Space** to spawn more boids

## Details
Pure ECS with parallel jobs. Can handle around 6k boids @ 60 FSP and almost 100% CPU usage

Configuration parameters can be tweaked in Resources/SteeringConfig

Spatial hashing algorithm is described here http://www.cs.ucf.edu/~jmesit/publications/scsc%202005.pdf

The order of the systems is as follows:

ForwardRaycast | SpatialHashing -> BoidFlocking -> BoidMovement
