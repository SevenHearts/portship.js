meta:
  id: rose_chr
  file-extension: chr
  endian: le
seq:
  - id: mesh_count
    type: u2
  - id: mesh_paths
    type: strz
    encoding: ascii
    repeat: expr
    repeat-expr: mesh_count
  - id: motion_count
    type: u2
  - id: motion_paths
    type: strz
    encoding: ascii
    repeat: expr
    repeat-expr: motion_count
  - id: effect_count
    type: u2
  - id: effect_paths
    type: strz
    encoding: ascii
    repeat: expr
    repeat-expr: effect_count
  - id: character_count
    type: u2
  - id: characters
    type: character
    repeat: expr
    repeat-expr: character_count
types:
  character:
    seq:
      - id: is_active
        type: u1
      - id: info
        type: character_info
        if: is_active != 0
    types:
      character_info:
        seq:
          - id: bone_id
            type: u2
          - id: name
            type: strz
            encoding: ascii
          - id: mesh_count
            type: u2
          - id: mesh_ids
            type: u2
            repeat: expr
            repeat-expr: mesh_count
          - id: motion_count
            type: u2
          - id: motions
            type: motion_info
            repeat: expr
            repeat-expr: motion_count
          - id: effect_count
            type: u2
          - id: effects
            type: effect_info
            repeat: expr
            repeat-expr: effect_count
        types:
          motion_info:
            seq:
              - id: id
                type: u2
              - id: motion_id
                type: u2
          effect_info:
            seq:
              - id: id
                type: u2
              - id: effect_id
                type: u2
