@echo off
REM fxc /T ps_5_0 /Fo Shaders\InfiniteGridPS.cso CodeWalker.Shaders\InfiniteGridPS.hlsl
fxc /T ps_5_0 /Fo Shaders\BlitVS.cso CodeWalker.Shaders\BlitVS.hlsl
fxc /T ps_5_0 /Fo Shaders\BlitPS.cso CodeWalker.Shaders\BlitPS.hlsl
