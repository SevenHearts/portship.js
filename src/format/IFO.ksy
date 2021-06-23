meta:
  id: rose_ifo
  file-extension: ifo
  endian: le
seq:
  - id: block_count
    type: u4
  - id: blocks
    type: block_info
    repeat: expr
    repeat-expr: block_count
types:
  block_info:
    seq:
      - id: type
        type: u4
        enum: block_type
      - id: offset
        type: u4
    instances:
      data:
        pos: offset
        type:
          switch-on: type
          cases:
            'block_type::economydata': block_economydata
            'block_type::decorations': entity_list
            'block_type::npcspawns': block_npcspawns
            'block_type::buildings': entity_list
            'block_type::soundeffects': block_soundeffects
            'block_type::effects': block_effects
            'block_type::animatables': entity_list
            'block_type::waterbig': block_waterbig
            'block_type::monsterspawns': block_monsterspawns
            'block_type::waterplanes': block_waterplanes
            'block_type::warpgates': entity_list
            'block_type::collisionblock': entity_list
            'block_type::triggers': block_triggers
    enums:
      block_type:
        0: economydata
        1: decorations
        2: npcspawns
        3: buildings
        4: soundeffects
        5: effects
        6: animatables
        7: waterbig
        8: monsterspawns
        9: waterplanes
        10: warpgates
        11: collisionblock
        12: triggers
    types:
      bstr:
        seq:
          - id: len
            type: u1
          - id: data
            type: str
            encoding: ascii
            size: len
      matrix4x4:
        seq:
          - id: components
            type: f4
            repeat: expr
            repeat-expr: 16
      quaternion:
        seq:
          - id: x
            type: f4
          - id: y
            type: f4
          - id: z
            type: f4
          - id: w
            type: f4
      vec3:
        seq:
          - id: x
            type: f4
          - id: y
            type: f4
          - id: z
            type: f4
      entity_list:
        seq:
          - id: entry_count
            type: u4
          - id: entities
            type: entity_info
            repeat: expr
            repeat-expr: entry_count
        types:
          entity_info:
            seq:
              - id: str_data
                type: bstr
              - id: warp_id
                type: u2
              - id: event_id
                type: u2
              - id: obj_type
                type: u4
              - id: obj_id
                type: u4
              - id: map_pos_x
                type: u4
              - id: map_pos_y
                type: u4
              - id: rotation
                type: quaternion
              - id: position
                type: vec3
              - id: scale
                type: vec3
      block_economydata:
        seq:
          - id: width
            type: u4
          - id: height
            type: u4
          - id: map_cell_x
            type: u4
          - id: map_cell_y
            type: u4
          - id: unused
            type: matrix4x4
          - id: block_name
            type: bstr
      block_npcspawns:
        seq:
          - id: entity_list
            type: entity_list
          - id: ai_pattern_index
            type: u4
          - id: con_file
            type: bstr
      block_soundeffects:
        seq:
          - id: entity_list
            type: entity_list
          - id: path
            type: bstr
          - id: range
            type: u4
          - id: interval
            type: u4
      block_effects:
        seq:
          - id: entity_list
            type: entity_list
          - id: path
            type: bstr
      block_waterbig:
        seq:
          - id: x_count
            type: u4
          - id: y_count
            type: u4
          - id: waters
            type: water_info
            repeat: expr
            repeat-expr: x_count * y_count
        types:
          water_info:
            seq:
              - id: use
                type: u1
              - id: height
                type: f4
              - id: water_type
                type: u4
              - id: water_index
                type: u4
              - id: reserved
                type: u4
      block_monsterspawns:
        seq:
          # Docs claim this but I don't think it's true.
          #- id: entity_list
          #  type: entity_list
          - id: spawn_count
            type: u4
          - id: spawns
            type: spawn_info
            repeat: expr
            repeat-expr: spawn_count
        types:
          spawn_info:
            seq:
              - id: name
                type: bstr
              - id: basicmob_count
                type: u4
              - id: basicmobs
                type: mob_info
                repeat: expr
                repeat-expr: basicmob_count
              - id: tacticmob_count
                type: u4
              - id: tacticmobs
                type: mob_info
                repeat: expr
                repeat-expr: tacticmob_count
              - id: interval
                type: u4
              - id: limit_count
                type: u4
              - id: range
                type: u4
              - id: tactic_points
                type: u4
            types:
              mob_info:
                seq:
                  - id: name
                    type: bstr
                  - id: monster_id
                    type: u4
                  - id: amount
                    type: u4
      block_waterplanes:
        seq:
          # Docs claim this is a list of some sort
          # but it makes no sense; the 4 bytes at the
          # offset are clearly a float, not a u4.
          #
          # Then there appears to be a u4 following it
          # that, when 1, has two vec3's following it.
          - id: unknown
            type: f4
          - id: extra_unknown
            type: u4
          - id: start
            type: vec3
            if: extra_unknown != 0
          - id: end
            type: vec3
            if: extra_unknown != 0
      block_triggers:
        seq:
          # This doesn't make any sense either.
          #- id: entity_list
          #  type: entity_list
          - id: qsd_trigger
            type: bstr
          - id: lua_trigger
            type: bstr
          # There are two bytes left over; are the b-strings
          # NUL-terminated?
