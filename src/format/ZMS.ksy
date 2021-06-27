meta:
  id: rose_zms
  file-extension: zms
  endian: le
seq:
  - id: magic
    contents: [0x5a, 0x4d, 0x53, 0x30]
  - id: version
    type: u4
    enum: zms_version
  - id: format
    type: u4
    enum: vertex_format
  - id: min_bounds
    type: vec3
  - id: max_bounds
    type: vec3
  - id: data
    type:
      switch-on: version
      cases:
        'zms_version::v6': zms_6
        'zms_version::v7': zms_78
        'zms_version::v8': zms_78
        _: dummy
types:
  dummy: {}
  zms_6:
    seq:
      - id: bone_count
        type: u4
      - id: bones
        type: bone_info_v6
        repeat: expr
        repeat-expr: bone_count
      - id: vert_count
        type: u4
      - id: vertices
        type: i_vec3
        repeat: expr
        repeat-expr: vert_count
        if: 0 != (_parent.format.to_i & vertex_format::position.to_i)
      - id: vert_normals
        type: i_vec3
        repeat: expr
        repeat-expr: vert_count
        if: 0 != (_parent.format.to_i & vertex_format::normal.to_i)
      - id: vert_colors
        type: i_rgba
        repeat: expr
        repeat-expr: vert_count
        if: 0 != (_parent.format.to_i & vertex_format::color.to_i)
      - id: vert_bones
        type: bone_v6
        repeat: expr
        repeat-expr: vert_count
        if: (_parent.format.to_i & (vertex_format::boneindices.to_i | vertex_format::boneweights.to_i)) == (vertex_format::boneindices.to_i | vertex_format::boneweights.to_i)
      - id: vert_tangents
        type: i_vec3
        repeat: expr
        repeat-expr: vert_count
        if: 0 != (_parent.format.to_i & vertex_format::tangent.to_i)
      - id: vert_uv1coords
        type: i_vec2
        repeat: expr
        repeat-expr: vert_count
        if: 0 != (_parent.format.to_i & vertex_format::uvmap1.to_i)
      - id: vert_uv2coords
        type: i_vec2
        repeat: expr
        repeat-expr: vert_count
        if: 0 != (_parent.format.to_i & vertex_format::uvmap2.to_i)
      - id: vert_uv3coords
        type: i_vec2
        repeat: expr
        repeat-expr: vert_count
        if: 0 != (_parent.format.to_i & vertex_format::uvmap3.to_i)
      - id: vert_uv4coords
        type: i_vec2
        repeat: expr
        repeat-expr: vert_count
        if: 0 != (_parent.format.to_i & vertex_format::uvmap4.to_i)
      - id: face_count
        type: u4
      - id: faces
        type: face_v6
        repeat: expr
        repeat-expr: face_count
      - id: material_count
        type: u4
      - id: materials
        type: i_mat_v6
        repeat: expr
        repeat-expr: material_count
    types:
      face_v6:
        seq:
          - type: u4 # skipped index
          - id: verts
            type: u4
            repeat: expr
            repeat-expr: 3
      i_mat_v6:
        seq:
          - type: u4 # skipped index
          - id: mat_id
            type: u4
      i_vec2:
        seq:
          - type: u4 # skipped index
          - id: x
            type: f4
          - id: y
            type: f4
      i_vec3:
        seq:
          - type: u4 # skipped index
          - id: x
            type: f4
          - id: y
            type: f4
          - id: z
            type: f4
      i_rgba:
        seq:
          - type: u4 # skipped index
          - id: r
            type: u1
          - id: g
            type: u1
          - id: b
            type: u1
          - id: a
            type: u1
      bone_info_v6:
        seq:
          - type: u4 # skipped index
          - id: bone_index
            type: u4
      bone_v6:
        seq:
          - type: u4 # skipped index
          - id: weights
            type: f4
            repeat: expr
            repeat-expr: 4
          - id: indices
            type: u4
            repeat: expr
            repeat-expr: 4
  zms_78:
    seq:
      - id: bone_count
        type: u2
      - id: bone_ids
        type: u2
        repeat: expr
        repeat-expr: bone_count
      - id: vert_count
        type: u2
      - id: vert_positions
        type: vec3
        repeat: expr
        repeat-expr: vert_count
        if: 0 != (_parent.format.to_i & vertex_format::position.to_i)
      - id: vert_normals
        type: vec3
        repeat: expr
        repeat-expr: vert_count
        if: 0 != (_parent.format.to_i & vertex_format::normal.to_i)
      - id: vert_colors
        type: rgba
        repeat: expr
        repeat-expr: vert_count
        if: 0 != (_parent.format.to_i & vertex_format::color.to_i)
      - id: vert_bones
        type: vert_bone
        repeat: expr
        repeat-expr: vert_count
        if: (_parent.format.to_i & (vertex_format::boneindices.to_i | vertex_format::boneweights.to_i)) == (vertex_format::boneindices.to_i | vertex_format::boneweights.to_i)
      - id: vert_tangents
        type: vec3
        repeat: expr
        repeat-expr: vert_count
        if: 0 != (_parent.format.to_i & vertex_format::tangent.to_i)
      - id: vert_uv1coords
        type: vec2
        repeat: expr
        repeat-expr: vert_count
        if: 0 != (_parent.format.to_i & vertex_format::uvmap1.to_i)
      - id: vert_uv2coords
        type: vec2
        repeat: expr
        repeat-expr: vert_count
        if: 0 != (_parent.format.to_i & vertex_format::uvmap2.to_i)
      - id: vert_uv3coords
        type: vec2
        repeat: expr
        repeat-expr: vert_count
        if: 0 != (_parent.format.to_i & vertex_format::uvmap3.to_i)
      - id: vert_uv4coords
        type: vec2
        repeat: expr
        repeat-expr: vert_count
        if: 0 != (_parent.format.to_i & vertex_format::uvmap4.to_i)
      - id: face_count
        type: u2
      - id: faces
        type: face
        repeat: expr
        repeat-expr: face_count
      - id: strip_count
        type: u2
      - id: strip_verts
        type: u2
        repeat: expr
        repeat-expr: strip_count
      - id: mat_type
        type: u2
  face:
    seq:
      - id: verts
        type: u2
        repeat: expr
        repeat-expr: 3
  rgba:
    seq:
      - id: r
        type: u1
      - id: g
        type: u1
      - id: b
        type: u1
      - id: a
        type: u1
  vec3:
    seq:
      - id: x
        type: f4
      - id: y
        type: f4
      - id: z
        type: f4
  vec2:
    seq:
      - id: x
        type: f4
      - id: y
        type: f4
  vert_bone:
    seq:
      - id: weights
        type: f4
        repeat: expr
        repeat-expr: 4
      - id: indices
        type: u2
        repeat: expr
        repeat-expr: 4
enums:
  zms_version:
    0x00363030: 'v6'
    0x00373030: 'v7'
    0x00383030: 'v8'
  vertex_format:
    2: position
    4: normal
    8: color
    16: boneindices
    32: boneweights
    64: tangent
    128: uvmap1
    256: uvmap2
    512: uvmap3
    1024: uvmap4
