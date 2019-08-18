"""voxelizes a mesh.
    Inputs:
        M: Mesh to be voxelized
        S: Length of each voxel (Positive float)
    Output:
        V: Voxels as a collection of breps
        P: Center points of each voxel
        """
        
import Grasshopper, GhPython
import System
import Rhino
import rhinoscriptsyntax as rs


import Rhino.Geometry as rg
import math

tmp = []

V = []
P = []

mesh = M
unit = S

if isinstance(mesh, rg.Mesh):
    
    mesh.UnifyNormals()
    mesh.Weld(math.pi)

    box = mesh.GetBoundingBox(rg.Plane.WorldXY)
    
    min_x = 9999999999999
    max_x = -min_x
    min_y = 9999999999999
    max_y = -min_y
    min_z = 9999999999999
    max_z = -min_z
    
    pts = box.GetCorners()
    
    for p in pts:
    
        if p.X > max_x:
            
            max_x = p.X
        
        if p.X < min_x:
            
            min_x = p.X
    
        if p.Y > max_y:
            
            max_y = p.Y
        
        if p.Y < min_y:
            
            min_y = p.Y
    
        if p.Z > max_z:
            
            max_z = p.Z
        
        if p.Z < min_z:
            
            min_z = p.Z

    if not unit:
                
        max_length = max(max_z - min_z, max(max_y - min_y, max_x - min_x))
                
        unit = int(max_length / 10)

    unit_interval = rg.Interval(-unit * 0.5, unit * 0.5)

    num_x = int((max_x - min_x) / unit)
    num_y = int((max_y - min_y) / unit)
    num_z = int((max_z - min_z) / unit)

    min_x = (max_x + min_x) * 0.5 - (num_x * unit * 0.5) 
    min_y = (max_y + min_y) * 0.5 - (num_y * unit * 0.5) 
    min_z = (max_z + min_z) * 0.5 - (num_z * unit * 0.5)
    
    horizontal_curves = {}
    vertical_curves   = {}
    
    horizontal_planes = {}
    vertical_planes   = {}
    
    for i in range(num_z + 1):
        
        z = min_z + i * unit
        
        p = rg.Point3d(0, 0, z)
        plane = rg.Plane(p, rg.Plane.WorldXY.Normal)
        
        curves = rg.Mesh.CreateContourCurves(mesh, plane)
        vertical_curves[z] = rg.Curve.JoinCurves(curves)
        vertical_planes[z] = plane

    
    for i in range(num_x + 1):
        
        x = min_x + i * unit
        
        p = rg.Point3d(x, 0, 0)
        plane = rg.Plane(p, rg.Plane.WorldXY.XAxis)
        
        curves = rg.Mesh.CreateContourCurves(mesh, plane)
        horizontal_curves[x] = rg.Curve.JoinCurves(curves)
        horizontal_planes[z] = plane

    reference_min_pt = rg.Point3d(0, 0, min_z-99999)
    reference_max_pt = rg.Point3d(0, 0, max_z+99999)
    
    for x in horizontal_curves:
        
        for curve in horizontal_curves[x]:
    
            if not isinstance(curve, rg.Polyline):
                
                curve = curve.ToPolyline()
    
            if not curve.IsClosed:
                
                curve.Add(curve.PointAt(0))
    
            min_pt = curve.ClosestPoint(reference_min_pt)
            max_pt = curve.ClosestPoint(reference_max_pt)

            for z in vertical_curves:

                if z < min_pt.Z or max_pt.Z < z:
                    
                    continue

                plane = vertical_planes[z]
                
                curve = curve.ToNurbsCurve()
                intersections = rg.Intersect.Intersection.CurvePlane(curve, plane, unit * 0.001)
                
                if len(intersections) < 2:
                    continue
                
                ys = []
                    
                for intersect in intersections:
                    
                    has_point = False
                    
                    for y in ys:
                        if y.DistanceTo(intersect.PointA) < 0.01:
                            has_point = True
                    
                    if not has_point:
                        ys.append(intersect.PointA)
                                
                ys = sorted(ys, key=lambda p: p.Y)  
                    
                num = int(len(ys) / 2)
                
                for i in range(num):
                    
                    local_min_y = ys[i * 2]
                    local_max_y = ys[i * 2 + 1]
                                        
                    # rather than seeing the distance, see the grid is inside or not
                    start_index = int((local_min_y.Y - min_y) / unit) + 1
                    end_index   = int((local_max_y.Y - min_y) / unit) + 1
                    
                    for y_index in range(start_index, end_index):
                        
                        y = min_y + unit * y_index
                        
                        grid_pt = rg.Point3d(x, y, z)
                    
                        grid_plane = rg.Plane(grid_pt, rg.Plane.WorldXY.ZAxis)
                        box = rg.Box(grid_plane, unit_interval, unit_interval, unit_interval)
                        V.append(box)
                        P.append(grid_pt)
        
