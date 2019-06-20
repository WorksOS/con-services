# Pull in the standard API hooks 
require 'sketchup.rb'
#require 'delaunay2.rb'

def create_contour_triangles( filename, group_name, color_name, alpha )

	contours = []

	IO.foreach(filename) { |line|
		#puts "line: #{line}"
		points = []
		line.each_line(';') { |s|
			s.scan(/(-?\d+\.\d+),(-?\d+\.\d+),(-?\d+\.\d+)/) { |x,y,z|
				#puts "x: #{x}  y: #{y}  z: #{z}"
				points << Geom::Point3d.new(x.to_f.m, y.to_f.m, z.to_f.m)
			}
		}
		contours << points
	}
	puts "Finished Parsing: #{contours.length} contours"

	if contours.length > 0
		create_shell_geometry(group_name, color_name, alpha, contours)
	end
end

def create_shell_geometry(group_name, color_name, alpha, contours = nil)

	model = Sketchup.active_model
	materials = model.materials

	layers = model.layers
	layer = layers.add group_name
	model.active_layer = layer

	group = model.entities.add_group
	group.name = group_name

	points = []
	contours.each { |c| 
		# Add all points in contour (except last to avoid duplication)
		pts = c[0,c.length-1].collect { |p| [p.x, p.y, p.z] }
		points.concat( pts )
	}

	puts "points: #{points.length}"

	# Triangulate
	triangles = triangulate(points)

	#create sketchup points
	sups = points.collect { |p| Geom::Point3d.new(p[0], p[1], p[2]) }
	mesh = Geom::PolygonMesh.new( sups.length, triangles.length)

	triangles.each { |t| mesh.add_polygon(sups[t[0]], sups[t[1]], sups[t[2]]) }

	puts "mesh: #{mesh.points.length}"
	
	# Create surface
	mat = materials.add color_name
	mat.alpha = alpha.to_f
	mat.color = color_name
	group.entities.add_faces_from_mesh mesh, 0, mat, mat
end

def store_model_latlon( filename )

   lines = IO.readlines(filename)
   if lines.length == 0
	 return nil
   end

   values = lines[0].split(',')
   if values.length < 2
	 return nil;
   end

   shadow_info = Sketchup.active_model.shadow_info
   shadow_info["Country"] = ""
   shadow_info["City"] = ""
   shadow_info["Latitude"] = values[1].to_f
   shadow_info["Longitude"] = values[0].to_f
end

def create_polyline( filename, group_name, color = nil )
     
	model = Sketchup.active_model

	layers = model.layers
	layer = layers.add group_name
	model.active_layer = layer

	group = model.entities.add_group
	group.name = group_name

	verts = []
	IO.foreach(filename) { |line|
		#puts "line: #{line[0]}"
		if ( line[0,1] == 'v')
			sline = line.split(':')
			sline[1].each_line(';') { |s|
				s.scan(/(-?\d+\.\d+),(-?\d+\.\d+),(-?\d+\.\d+)/) { |x,y,z|
					#puts "x: #{x}  y: #{y}  z: #{z}"
					fz = z.to_f.m
					verts << Geom::Point3d.new(x.to_f.m, y.to_f.m, fz)
				}
			}
		end
	}
	if verts.length > 0
		edges = group.entities.add_edges(verts)
		if not color.nil?
			model.rendering_options["EdgeColorMode"] = 0  #By material
			mat = model.materials.add color
			mat.color = color
			edges.each { |edge|
				edge.material = mat
			}
		end
	end
end

def create_linestring( filename, group_name, color = nil )
     
	model = Sketchup.active_model

	layers = model.layers
	layer = layers.add group_name
	model.active_layer = layer

	group = model.entities.add_group
	group.name = group_name

	lines = []
	IO.foreach(filename) { |line|
		#puts "line: #{line[0]}"
		if ( line[0,1] == 'l' )
			sline = line.split(':')
			pts = []
			sline[1].each_line(' ') { |s|
				s.scan(/(-?\d+\.\d+),(-?\d+\.\d+),(-?\d+\.\d+)/) { |x,y,z|
					#puts "x: #{x}  y: #{y}  z: #{z}"
					pts << Geom::Point3d.new(x.to_f.m, y.to_f.m, z.to_f.m)
				}
			}
			lines << pts
			#puts "line: #{line[0]}"
		end
	}
	lines.each { |l| 
		edge = group.entities.add_edges(l)
		if not color.nil?
			model.rendering_options["EdgeColorMode"] = 0  #By material
			mat = model.materials.add color
			mat.color = color
			edge.each { |e|
				e.material = mat
			}
		end
	}
end

def get_material_from_file(mat_name, filename)
	if not File.exist?(filename)
		return nil
	end
	mat = Sketchup.active_model.materials.add mat_name
	mat.texture = filename
	return mat
end

def create_mesh( filename, group_name, color_name, alpha, texture_filename = nil )
     
	model = Sketchup.active_model
	materials = model.materials

	layers = model.layers
	layer = layers.add group_name
	model.active_layer = layer

	group = model.entities.add_group
	group.name = group_name

	verts = []
	faces = []
	min_z = nil
	max_z = nil
	IO.foreach(filename) { |line|
		#puts "line: #{line[0,1]}"
		if ( line[0,1] == 'v')
			sline = line.split(':')
			sline[1].each_line(';') { |s|
				s.scan(/(-?\d+\.\d+),(-?\d+\.\d+),(-?\d+\.\d+)/) { |x,y,z|
					#puts "x: #{x}  y: #{y}  z: #{z}"
					fz = z.to_f.m
					verts << Geom::Point3d.new(x.to_f.m, y.to_f.m, fz)
					if min_z.nil? || fz < min_z
						min_z = fz
					end
					if max_z.nil? || fz > max_z
						max_z = fz
					end
				}
			}
		elsif ( line[0,1] == 'f')
			sline = line.split(':')
			sline[1].each_line(';') { |s|
				s.scan(/(-?\d+),(-?\d+),(-?\d+)/) { |a,b,c|
					#puts "a: #{a}  b: #{b}  c: #{c}"
					faces << a.to_i
					faces << b.to_i
					faces << c.to_i
				}
			}
			#if faces.length >= 3*15000
			#	break
			#end
		end
	}
	puts "done with file parsing"

	mesh = Geom::PolygonMesh.new( verts.length, faces.length / 3)

	i = 0
	puts "#{faces.length/3} faces"
	until i >= (faces.length-1)
		if !(verts[faces[i]] == verts[faces[i+1]]) && !(verts[faces[i]] == verts[faces[i+2]]) && !(verts[faces[i+1]] == verts[faces[i+2]])
			mesh.add_polygon(verts[faces[i]], verts[faces[i+1]], verts[faces[i+2]])
			#puts "p1: #{verts[faces[i]]}  p2: #{verts[faces[i+1]]}  p3: #{verts[faces[i+2]]}"
		end
		i += 3
	end
	puts "done with adding faces to mesh"

	myMat = nil
	if not texture_filename.nil?
		myMat = get_material_from_file "AltMap", texture_filename
	elsif not color_name.nil?
		myMat = materials.add color_name
		myMat.color = color_name
	end
	if not myMat.nil?
		myMat.alpha = alpha.to_f
	end

	puts "adding faces from mesh"
	if myMat.nil?
		puts "adding, no material"
		group.entities.add_faces_from_mesh mesh, 0
		puts "added, no material"
	else
		if myMat.texture.nil?
			puts "adding: material, no texture"
			group.entities.add_faces_from_mesh mesh, 0, myMat, myMat
			puts "added: material, no texture"
		else
			puts "adding: material and texture"
			# Inflate range to prevent wraparound at extremes
			top = [min_z.abs, max_z.abs].max * 1.01
			bottom = -top
			height = top - bottom
			puts "bottom: #{bottom}, height: #{height}"
			group.entities.add_faces_from_mesh mesh, 0
			puts "added: material and texture"
			group.entities.each { |ent|
				if ent.is_a? Sketchup::Face
					z_plane_normal = Geom::Vector3d.new(-ent.normal.y, ent.normal.x, 0)
					pt_array = []
					if (z_plane_normal.length + 0) < 0.003
						# Face is horizontal
						v = 0.5
						if height > 0
							v = (ent.vertices[0].position.z / height) + 0.5
						end
						pt_array.push ent.vertices[0].position
						pt_array.push Geom::Point3d.new(0, v, 0)
						pt_array.push ent.vertices[1].position
						pt_array.push Geom::Point3d.new(0.001, v, 0)
					else
						u = 0
						ent.vertices.sort { |v1, v2| v1.position.z <=> v2.position.z }.each { |vertex|
							pt_array.push vertex.position
							v = 0.5
							if height > 0
								v = (vertex.position.z / height) + 0.5
							end
							pt_array.push Geom::Point3d.new(u, v, 0)
							u += 0.5
						}
					end
					begin
						ent.position_material myMat, pt_array, true
						ent.position_material myMat, pt_array, false
					rescue => e
						puts "z_plane_normal: #{z_plane_normal.inspect}. Plane normal length: #{z_plane_normal.length+0}"
						puts "pt_array: #{pt_array}"
						puts e.message
					end
				end
			}
		end
	end

	puts "done with face creation"
end

def create_planes( filename, group_name, color_name, alpha )
     
	model = Sketchup.active_model
	materials = model.materials

	layers = model.layers
	layer = layers.add group_name
	model.active_layer = layer

	group = model.entities.add_group
	group.name = group_name

	myMat = materials.add color_name
	myMat.color = color_name
	myMat.alpha = alpha.to_f

	IO.foreach(filename) { |line|
		#puts "line: #{line[0]}"
		#if ( line[0,1] == 'p' )
			data = line.split(':')
			verts = []
			data[1].each_line(' ') { |s|
				s.scan(/(-?\d+\.\d+),(-?\d+\.\d+),(-?\d+\.\d+)/) { |x,y,z|
					verts << Geom::Point3d.new(x.to_f.m, y.to_f.m, z.to_f.m)
				}
			}

			#puts "verts: #{verts.length}"
			f = group.entities.add_face(verts)  
			f.material = myMat 

		#end
	}
	puts "done with plane creation"
end

def get_plugin_resource(filename)
	#extract this plugin full path
	$LOAD_PATH.each { |d| 
	    f = File.join(d, filename)
		if File.exist?(f)
			return f
		end
	}
	return filename
end

def run_plugin(folder)
  model = Sketchup.active_model
  status = model.start_operation("Export from WM-Form", true)
	puts "#{folder}"
	file = folder + '\\surface.txt'
	if ( File.exist?(file) )
		create_mesh( file, "surface", "BurlyWood", "0.75" )
	end
	file = folder + '\\planes.txt'
	if ( File.exist?(file) )
		create_planes( folder + '\\planes.txt' , "planes", "Red", "1" )
	end
	file = folder + '\\modelorigin.txt'
	if ( File.exist?(file) )
		store_model_latlon( folder + '\\modelorigin.txt' )
	end
	file = folder + '\\design.txt'
	if ( File.exist?(file) )
		create_mesh( file, "design", "Green", "0.75" )
	end
	before = Time.now
	file = folder + '\\cutfill.txt';
	if ( File.exist?(file) )
		filename = get_plugin_resource('AltitudeTexture.png')
		create_mesh( file, "cutfill", nil, "0.75", filename )
	end
	elapsed = Time.now - before
	puts "Elapsed: #{elapsed}"
	file = folder + '\\rows.txt';
	if ( File.exist?(file) )
		create_linestring( file, "rows", "lime" )
	end
	file = folder + '\\ditch.txt';
	if ( File.exist?(file) )
		create_polyline( file, "ditch", "lime" )
	end
	file = folder + '\\pipeline.txt';
	if ( File.exist?(file) )
		create_polyline( file, "pipeline", "lime" )
	end
  model.commit_operation
  Sketchup.active_model.active_view.zoom_extents
end

