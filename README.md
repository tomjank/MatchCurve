# MatchCurve
A simple Grasshopper Component to match a curve to another curve up to G2 continuity.

This is a simple Grasshopper Plugin. It requires Rhino 6. If you like to use it in Rhino 5 you need to build it with .Net 4.0 instead of .Net 4.5.

All operations can be easily made GH independent. It requires a Point structure, a Nurbs- or Beziercurve with the ability of degree elevation, Frenet Frame to get Point, Tangent and Curvature of such a curve. It also needs to compute the curvature at a given parameter. It should support at least cubic Beziers.

It can also be made as a direct Rhino Plugin. However the native Match Curve command supports higher continuity.
You will find a note on how to match Flow, but its not implement yet. Its not planned to actually develop further, but feel
free to participate. If you use this for your own tools, I would be happy if you credit me.

See the 'Binary' folder for the .gha and a .gh file to test.

Developed in 2017 by Tom Jankowski, made public in May 2020.

Please see: 

https://discourse.mcneel.com/t/matchcrv-in-gh/58835/7
https://discourse.mcneel.com/t/control-points-math-relation-of-g2-continuity/102710
