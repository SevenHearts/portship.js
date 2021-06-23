meta:
  id: rose_zon
  file-extension: zon
  endian: le
seq:
  - id: block_count
    type: u4
  - id: blocks
    type: zone_block
    repeat: expr
    repeat-expr: block_count
types:
  zone_block:
    seq:
      - id: id
        type: u4
        enum: block_type
      - id: offset
        type: u4
    enums:
      block_type:
        0: basic_info
        1: event_points
        2: texture_list
        3: tile_list
        4: wtf_info
    instances:
      data:
        pos: offset
        type:
          switch-on: id
          cases:
            'block_type::basic_info': block_basic_info
            'block_type::event_points': block_event_points
            'block_type::texture_list': block_texture_list
            'block_type::tile_list': block_tile_list
            'block_type::wtf_info': block_wtf_info
    types:
      block_basic_info:
        seq:
          - id: zone_type
            type: u4
          - id: zone_width
            type: u4
          - id: zone_height
            type: u4
          - id: grid_count
            type: u4
          - id: grid_size
            type: f4
          - id: x_count
            type: u4
          - id: y_count
            type: u4
          - id: zones
            type: zone_info
            repeat: expr
            repeat-expr: zone_width * zone_height
        types:
          zone_info:
            seq:
              - id: use_map
                type: b1
              - id: x
                type: f4
              - id: y
                type: f4
      block_event_points:
        seq:
          - id: entry_count
            type: u4
          - id: event_points
            type: event_point
            repeat: expr
            repeat-expr: entry_count
        types:
          event_point:
            seq:
              - id: x
                type: f4
              - id: z
                type: f4
              - id: y
                type: f4
              - id: name
                type: bstr
      block_texture_list:
        seq:
          - id: entry_count
            type: u4
          - id: textures
            type: bstr
            repeat: expr
            repeat-expr: entry_count
      block_tile_list:
        seq:
          - id: entry_count
            type: u4
          - id: tiles
            type: tile_info
            repeat: expr
            repeat-expr: entry_count
        types:
          tile_info:
            seq:
              - id: base1
                type: u4
              - id: base2
                type: u4
              - id: offset1
                type: u4
              - id: offset2
                type: u4
              - id: is_blending
                type: u4
              - id: orientation
                type: u4
              - id: tile_type
                type: u4
      block_wtf_info:
        seq:
          - id: area_name
            type: bstr
          - id: is_underground
            type: u4
          - id: button_bgm
            type: bstr
          - id: button_back
            type: bstr
          - id: check_count
            type: u4
          - id: standard_population
            type: u4
          - id: standard_growth_rate
            type: u4
          - id: metal_consumption
            type: u4
          - id: stone_consumption
            type: u4
          - id: wood_consumption
            type: u4
          - id: leather_consumption
            type: u4
          - id: cloth_consumption
            type: u4
          - id: alchemy_consumption
            type: u4
          - id: chemical_consumption
            type: u4
          - id: industrial_consumption
            type: u4
          - id: medicine_consumption
            type: u4
          - id: food_consumption
            type: u4
      bstr:
        seq:
          - id: len
            type: u1
          - id: data
            type: str
            encoding: ascii
            size: len
