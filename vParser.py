# f {vertex index}/{vertex texture coordinate index}/{vertex normal index}
# don't forget to triangulate the model before saving as obj.
v = [] #list of sets of coordinates of shape vertices
vn = [] #list of sets of vertex normals
vt = [] #list of sets of texture coordinates
f = [] #list of faces
finalVertices = []
finalIndices = [] # was actually not needed in the end, but left it here anyway
tangentSet = []

def appendColor():
    finalVertices.append(str(1) + '.0')
    finalVertices.append(str(1) + '.0')
    finalVertices.append(str(1) + '.0')

def vec3Subtract(a, b):
    ax = a[0]
    ay = a[1]
    az = a[2]

    bx = b[0]
    by = b[1]
    bz = b[2]

    final = []
    final.append(ax - bx)
    final.append(ay - by)
    final.append(az - bz)

    return final

def vec2Subtract(a, b):
    ax = a[0]
    ay = a[1]

    bx = b[0]
    by = b[1]

    final = []
    final.append(ax - bx)
    final.append(ay - by)

    return final

def parseVertices(vertices):
    objfile = open(vertices)
    


    for line in objfile:
        if(line[0] == "v" and line[1] == " "):
            x = line.split()
            if("v" in x):
                x.remove("v")
            y = [float(vertices) for vertices in x]
            v.append(y)
        elif(line[0] == "v" and line[1] == "n"):
            x = line.split()
            if("vn" in x):
                x.remove("vn")
            y = [float(vertNormals) for vertNormals in x]
            vn.append(y)
        elif(line[0] == "v" and line[1] == "t"): # texture coordinates
            x = line.split()
            if("vt" in x):
                x.remove("vt")
            y = [float(texCoords) for texCoords in x]
            vt.append(y)
        elif(line[0] == "f" and line[1] == " "): # change to support the faces which include the textures
            lineReplaced = line.replace('/',' ')
            x = lineReplaced.split()
            if("f" in x):
                x.remove("f")
            y = [int(faces) for faces in x]
            f.append(y)


    for faces in f:
        # vertex / texture / normal
        fv1_index = faces[0] # first point
        fvn1_index = faces[2]
        fvt1_index = faces[1]

        fv2_index = faces[3] # second point
        fvn2_index = faces[5]
        fvt2_index = faces[4]

        fv3_index = faces[6] # third point
        fvn3_index = faces[8]
        fvt3_index = faces[7]

        # forms an entire triangle

        # get tangents

        edge1 = vec3Subtract(v[fv2_index-1],v[fv1_index-1])
        edge2 = vec3Subtract(v[fv3_index-1],v[fv1_index-1])
        deltaUV1 = vec2Subtract(vt[fvt2_index-1],vt[fvt1_index-1])
        deltaUV2 = vec2Subtract(vt[fvt3_index-1],vt[fvt1_index-1])

        floatf = 1/ (deltaUV1[0] * deltaUV2[1] - deltaUV2[0] * deltaUV1[1])
        
        tanx = floatf * (deltaUV2[1] * edge1[0] - deltaUV1[1] * edge2[0]) # f * (deltaUV2.y * edge1.x - deltaUV1.y * edge2.x);
        tany = floatf * (deltaUV2[1] * edge1[1] - deltaUV1[1] * edge2[1])
        tanz = floatf * (deltaUV2[1] * edge1[2] - deltaUV1[1] * edge2[2])
        
        tangentSet.append(tanx)
        tangentSet.append(tany)
        tangentSet.append(tanz)

        # pair 1
        for coordinates in v[fv1_index-1]:
            finalVertices.append(coordinates)
        for coordinates in vn[fvn1_index-1]:
            finalVertices.append(coordinates)
        finalVertices.append(tanx)
        finalVertices.append(tany)
        finalVertices.append(tanz)
        for coordinates in vt[fvt1_index-1]:
            finalVertices.append(coordinates)

        # pair 2
        for coordinates in v[fv2_index-1]:
            finalVertices.append(coordinates)
        for coordinates in vn[fvn2_index-1]:
            finalVertices.append(coordinates)
        finalVertices.append(tanx)
        finalVertices.append(tany)
        finalVertices.append(tanz)
        for coordinates in vt[fvt2_index-1]:
            finalVertices.append(coordinates)

        # pair 3
        for coordinates in v[fv3_index-1]:
            finalVertices.append(coordinates)
        for coordinates in vn[fvn3_index-1]:
            finalVertices.append(coordinates)
        finalVertices.append(tanx)
        finalVertices.append(tany)
        finalVertices.append(tanz)
        for coordinates in vt[fvt3_index-1]:
            finalVertices.append(coordinates)
        
    
    with open('newVertices.txt','w') as newVerts:
        count = 0
        for e in finalVertices:
            newVerts.write(str(e) + 'f')
            count+=1
            newVerts.write(', ')
            if(count == 11):
                newVerts.write('\n')
                count=0
parseVertices("chikipi_new.obj")

print(tangentSet)