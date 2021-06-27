meta:
  id: rose_zsc
  file-extension: zsc
  endian: le
seq:
  - id: mesh_count
    type: u2
  - id: mesh_paths
    type: strz
    encoding: ascii
    repeat: expr
    repeat-expr: mesh_count
  - id: material_count
    type: u2
  - id: materials
    type: material
    repeat: expr
    repeat-expr: material_count
  - id: effect_count
    type: u2
  - id: effects
    type: strz
    encoding: ascii
    repeat: expr
    repeat-expr: effect_count
  - id: object_count
    type: u2
  - id: objects
    type: object_def
    repeat: expr
    repeat-expr: object_count
types:
  material:
    seq:
      - id: path
        type: strz
        encoding: ascii
      - id: is_skin
        type: u2
      - id: alpha_enabled
        type: u2
      - id: two_sided
        type: u2
      - id: alpha_test_enabled
        type: u2
      - id: alpha_ref_enabled
        type: u2
      - id: z_write_enabled
        type: u2
      - id: z_test_enabled
        type: u2
      - id: blending_mode
        type: u2
        enum: blend_mode
      - id: specular_enabled
        type: u2
      - id: alpha
        type: f4
      - id: glow
        type: u2
        enum: glow_type
      - id: red
        type: f4
      - id: green
        type: f4
      - id: blue
        type: f4
  object_def:
    seq:
      - id: boundingsphere_radius
        type: u4
      - id: boundingsphere_x
        type: s4
      - id: boundingsphere_y
        type: s4
      - id: mesh_count
        type: u2
      - id: meshes
        type: mesh_def
        repeat: expr
        repeat-expr: mesh_count
        if: 'mesh_count > 0'
      - id: effect_count
        type: u2
        if: 'mesh_count > 0'
      - id: effects
        type: effect_def
        repeat: expr
        repeat-expr: effect_count
        if: 'mesh_count > 0'
      - id: minbounds
        type: vec3
        if: 'mesh_count > 0'
      - id: maxbounds
        type: vec3
        if: 'mesh_count > 0'
  mesh_def:
    seq:
      - id: mesh_id
        type: u2
      - id: material_id
        type: u2
      - id: properties
        type: mesh_prop
        repeat: until
        repeat-until: '_.type == mesh_prop_type::end'
  effect_def:
    seq:
      - id: effect_id
        type: u2
      # Doesn't appear to be used anymore.
      # - id: type
      #   type: u2
      #   enum: effect_type
      - id: properties
        type: mesh_prop
        repeat: until
        repeat-until: '_.type == mesh_prop_type::end'
  mesh_prop:
    seq:
      - id: type
        type: u1
        enum: mesh_prop_type
      - id: size
        type: u1
        if: 'type != mesh_prop_type::end'
      - id: data
        # Technically this should be OK,
        # but Kaitai generates invalid C# code
        # if it's included.
        #size: size
        type:
          switch-on: type
          cases:
            'mesh_prop_type::end': prop_null
            'mesh_prop_type::position': vec3
            'mesh_prop_type::rotation': vec4
            'mesh_prop_type::scale': vec3
            'mesh_prop_type::axisrotation': vec4
            'mesh_prop_type::boneindex': u2
            'mesh_prop_type::dummyindex': u2
            'mesh_prop_type::parent': u2
            'mesh_prop_type::collision': collision_info
            'mesh_prop_type::zmopath': zmo_path
            'mesh_prop_type::rangemode': u2
            'mesh_prop_type::lightmapmode': u2
            _: prop_null
  prop_null: {}
  vec3:
    seq:
      - id: x
        type: f4
      - id: y
        type: f4
      - id: z
        type: f4
  vec4:
    seq:
      - id: w
        type: f4
      - id: x
        type: f4
      - id: y
        type: f4
      - id: z
        type: f4
  collision_info:
    seq:
      - id: raw
        type: u2
    instances:
      pick:
        value: '(raw & 0b1111111111111000) >> 3'
        enum: collisionpick_type
      type:
        value: 'raw & 0b111'
        enum: collision_type
  zmo_path:
    seq:
      - id: bytes
        type: u1
        repeat: expr
        repeat-expr: _parent.size
enums:
  mesh_prop_type:
    0x00: end
    0x01: position
    0x02: rotation
    0x03: scale
    0x04: axisrotation
    0x05: boneindex
    0x06: dummyindex
    0x07: parent
    0x1D: collision
    0x1E: zmopath
    0x1F: rangemode
    0x20: lightmapmode
  collision_type:
    0: none
    1: sphere
    2: aabb
    3: orientedbb
    4: polygon
  collisionpick_type:
    0x0: none
    0x1: notmovable
    0x2: notpickable
    0x4: heightonly
    0x8: nocameracollision
  effect_type:
    0: normal
    1: daynight
    2: lightcontainer
  blend_mode:
    0: none
    1: custom
    2: normal
    3: lighten
  glow_type:
    0: none
    1: notset
    2: simple
    3: light
    4: texture
    5: texturelight
    6: alpha
