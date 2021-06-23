meta:
  id: rose_aip
  file-extension: aip
  endian: le
seq:
  - id: trigger_count
    type: u4
  - id: idle_check
    type: u4
  - id: damage_check
    type: u4
  - id: title
    type: lstr
  - id: triggers
    type: trigger
    repeat: expr
    repeat-expr: trigger_count
types:
  lstr:
    seq:
      - id: len
        type: u4
      - id: data
        type: str
        encoding: ascii
        size: len
  trigger:
    seq:
      - id: trigger_name
        type: str
        size: 0x20
        encoding: ascii
      - id: block_count
        type: u4
      - id: blocks
        type: block
        repeat: expr
        repeat-expr: block_count
    types:
      block:
        seq:
          - id: block_name
            type: str
            encoding: ascii
            size: 0x20
          - id: condition_count
            type: u4
          - id: conditions
            type: command
            repeat: expr
            repeat-expr: condition_count
          - id: action_count
            type: u4
          - id: actions
            type: command
            repeat: expr
            repeat-expr: action_count
        enums:
          command_id:
            0x04000001: hold_or_attack
            0x04000002: damage
            0x04000003: check_near_1
            0x04000004: check_distance_1
            0x04000005: check_distance_2
            0x04000006: check_ab_1
            0x04000007: check_hp
            0x04000008: random_chance
            0x04000009: check_near_2
            0x0400000a: unknown
            0x0400000b: check_ab_2
            0x0400000c: check_ab_3
            0x0400000d: check_time_1
            0x0400000e: check_target_1
            0x0400000f: check_variable_1
            0x04000010: check_variable_2
            0x04000011: check_variable_3
            0x04000012: select_npc
            0x04000013: check_distance_3
            0x04000014: check_time_2
            0x04000015: check_ab_4
            0x04000016: unknown_1
            0x04000017: unknown_2
            0x04000018: check_time_3
            0x04000019: check_date_time_4
            0x0400001a: check_weekday_time_5
            0x0400001b: check_position
            0x0400001c: check_near_character
            0x0400001d: check_variable_4
            0x0400001e: check_target_2
            0x0400001f: unknown_3
            0x0B000001: unknown_4
            0x0B000002: do_action
            0x0B000003: say_ltb_string
            0x0B000004: move_1
            0x0B000005: move_2
            0x0B000006: move_3
            0x0B000007: move
            0x0B000008: unknown_5
            0x0B000009: move_4
            0x0B00000a: spawn_monster_1
            0x0B00000b: spawn_monster_2
            0x0B00000c: spawn_monster_3
            0x0B00000d: unknown_6
            0x0B00000e: unknown_7
            0x0B00000f: unknown_8
            0x0B000010: retaliate
            0x0B000011: unknown_9
            0x0B000012: drop_item
            0x0B000013: spawn_monster_4
            0x0B000014: unknown_10
            0x0B000015: spawn_monster_5
            0x0B000016: unknown_11
            0x0B000017: unknown_12
            0x0B000018: unknown_13
            0x0B000019: do_skill
            0x0B00001a: set_variable_1
            0x0B00001b: set_variable_2
            0x0B00001c: set_variable_3
            0x0B00001d: shout_ann_ltb_string
            0x0B00001e: unknown_14
            0x0B00001f: do_trigger
            0x0B000020: unknown_15
            0x0B000021: zone_1
            0x0B000022: zone_2
            0x0B000023: item
            0x0B000024: set_variable_4
            0x0B000025: monster_1
            0x0B000026: monster_2
        types:
          sstr:
            seq:
              - id: len
                type: u2
              - id: data
                type: str
                encoding: ascii
                size: len
          command:
            seq:
              - id: cmd_size
                type: u4
              - id: cmd_id
                type: u4
                enum: command_id
              - id: data
                size: cmd_size - 8
                type:
                  switch-on: cmd_id
                  cases:
                    'command_id::hold_or_attack': cmd_hold_or_attack
                    'command_id::damage': cmd_damage
                    'command_id::check_near_1': cmd_check_near_1
                    'command_id::check_distance_1': cmd_check_distance_1
                    'command_id::check_distance_2': cmd_check_distance_2
                    'command_id::check_ab_1': cmd_check_ab_1
                    'command_id::check_hp': cmd_check_hp
                    'command_id::random_chance': cmd_random_chance
                    'command_id::check_near_2': cmd_check_near_2
                    'command_id::check_ab_2': cmd_check_ab_2
                    'command_id::check_ab_3': cmd_check_ab_3
                    'command_id::check_time_1': cmd_check_time_1
                    'command_id::check_target_1': cmd_check_target_1
                    'command_id::check_variable_1': cmd_check_variable_1
                    'command_id::check_variable_2': cmd_check_variable_2
                    'command_id::select_npc': cmd_select_npc
                    'command_id::check_distance_3': cmd_check_distance_3
                    'command_id::check_time_2': cmd_check_time_2
                    'command_id::check_ab_4': cmd_check_ab_4
                    'command_id::check_time_3': cmd_check_time_3
                    'command_id::check_date_time_4': cmd_check_date_time_4
                    'command_id::check_weekday_time_5': cmd_check_weekday_time_5
                    'command_id::check_position': cmd_check_position
                    'command_id::check_near_character': cmd_check_near_character
                    'command_id::check_variable_4': cmd_check_variable_4
                    'command_id::check_target_2': cmd_check_target_2
                    'command_id::unknown_3': cmd_unknown_3
                    'command_id::do_action': cmd_do_action
                    'command_id::say_ltb_string': cmd_say_ltb_string
                    'command_id::move_1': cmd_move_1
                    'command_id::move_2': cmd_move_2
                    'command_id::move_3': cmd_move_3
                    'command_id::move': cmd_move
                    'command_id::move_4': cmd_move_4
                    'command_id::spawn_monster_1': cmd_spawn_monster_1
                    'command_id::spawn_monster_2': cmd_spawn_monster_2
                    'command_id::spawn_monster_3': cmd_spawn_monster_3
                    'command_id::unknown_8': cmd_unknown_8
                    'command_id::unknown_9': cmd_unknown_9
                    'command_id::drop_item': cmd_drop_item
                    'command_id::spawn_monster_4': cmd_spawn_monster_4
                    'command_id::spawn_monster_5': cmd_spawn_monster_5
                    'command_id::do_skill': cmd_do_skill
                    'command_id::set_variable_1': cmd_set_variable_1
                    'command_id::shout_ann_ltb_string': cmd_shout_ann_ltb_string
                    'command_id::do_trigger': cmd_do_trigger
                    'command_id::zone_1': cmd_zone_1
                    'command_id::item': cmd_item
                    'command_id::set_variable_4': cmd_set_variable_4
                    'command_id::monster_1': cmd_monster_1
                    'command_id::monster_2': cmd_monster_2
          cmd_hold_or_attack:
            seq:
              - id: c_notfight_or_delay
                type: u1
          cmd_damage:
            seq:
              - id: i_damage
                type: u4
              - id: c_recv_or_give
                type: u1
          cmd_check_near_1:
            seq:
              - id: i_distance
                type: u4
              - id: bt_is_allied
                type: u1
              - id: n_level_diff
                type: u2
              - id: n_level_diff2
                type: u2
              - id: w_chr_num
                type: u2
          cmd_check_distance_1:
            seq:
              - id: i_distance
                type: u4
          cmd_check_distance_2:
            seq:
              - id: i_distance
                type: u4
              - id: c_more_less
                type: u1
          cmd_check_ab_1:
            seq:
              - id: c_ab_type
                type: u1
              - id: i_diff
                type: u4
              - id: c_more_less
                type: u1
          cmd_check_hp:
            seq:
              - id: w_hp
                type: u4
              - id: c_more_less
                type: u1
          cmd_random_chance:
            seq:
              - id: c_percent
                type: u1
          cmd_check_near_2:
            seq:
              - id: i_distance
                type: u4
              - id: n_level_diff
                type: u2
              - id: n_level_diff2
                type: u2
              - id: bt_is_allied
                type: u1
          cmd_check_ab_2:
            seq:
              - id: c_ab_type
                type: u1
              - id: c_more_less
                type: u1
          cmd_check_ab_3:
            seq:
              - id: c_ab_type
                type: u1
              - id: i_value
                type: u4
              - id: c_more_less
                type: u1
          cmd_check_time_1:
            seq:
              - id: c_when
                type: u1
          cmd_check_target_1:
            seq:
              - id: bt_check_target
                type: u1
              - id: bt_status_type
                type: u1
              - id: bt_have
                type: u1
          cmd_check_variable_1:
            seq:
              - id: bt_var_idx
                type: u1
              - id: bt_op
                type: u1
              - id: i_value
                type: u4
          cmd_check_variable_2:
            seq:
              - id: n_var_idx
                type: u2
              - id: bt_op
                type: u1
              - id: i_value
                type: u4
          cmd_select_npc:
            seq:
              - id: i_npc_no
                type: u4
          cmd_check_distance_3:
            seq:
              - id: i_distance
                type: u4
              - id: bt_op
                type: u1
          cmd_check_time_2:
            seq:
              - id: ul_time
                type: u4
              - id: ul_end_time
                type: u4
          cmd_check_ab_4:
            seq:
              - id: bt_ab_type
                type: u1
              - id: bt_op
                type: u1
              - id: i_value
                type: u4
          cmd_check_time_3:
            seq:
              - id: ul_time
                type: u4
              - id: ul_end_time
                type: u4
          cmd_check_date_time_4:
            seq:
              - id: bt_date
                type: u1
              - id: bt_hour1
                type: u1
              - id: bt_min1
                type: u1
              - id: bt_hour2
                type: u1
              - id: bt_min2
                type: u1
          cmd_check_weekday_time_5:
            seq:
              - id: bt_week_day
                type: u1
              - id: bt_hour1
                type: u1
              - id: bt_min1
                type: u1
              - id: bt_hour2
                type: u1
              - id: bt_min2
                type: u1
          cmd_check_position:
            seq:
              - id: n_x
                type: u2
              - id: n_y
                type: u2
          cmd_check_near_character:
            seq:
              - id: i_distance
                type: u4
              - id: bt_is_allied
                type: u1
              - id: n_level_diff
                type: u2
              - id: n_level_diff2
                type: u2
              - id: w_chr_num
                type: u2
              - id: bt_op
                type: u1
          cmd_check_variable_4:
            seq:
              - id: n_var_idx
                type: u2
              - id: bt_op
                type: u1
              - id: i_value
                type: u4
          cmd_check_target_2:
            seq:
              - id: bt_target_type
                type: u1
          cmd_unknown_3:
            seq:
              - id: unknown
                type: u4
          cmd_do_action:
            seq:
              - id: c_action
                type: u1
          cmd_say_ltb_string:
            seq:
              - id: i_str_id
                type: u4
          cmd_move_1:
            seq:
              - id: i_distance
                type: u4
              - id: c_speed
                type: u1
          cmd_move_2:
            seq:
              - id: i_distance
                type: u4
              - id: c_speed
                type: u1
          cmd_move_3:
            seq:
              - id: c_speed
                type: u1
          cmd_move:
            seq:
              - id: i_distance
                type: u4
              - id: c_ab_type
                type: u1
              - id: c_more_less
                type: u1
          cmd_move_4:
            seq:
              - id: i_distance
                type: u4
              - id: c_speed
                type: u1
          cmd_spawn_monster_1:
            seq:
              - id: w_monster
                type: u2
          cmd_spawn_monster_2:
            seq:
              - id: w_monster
                type: u2
          cmd_spawn_monster_3:
            seq:
              - id: i_distance
                type: u4
              - id: i_num_of_monster
                type: u4
          cmd_unknown_8:
            seq:
              - id: i_distance
                type: u4
          cmd_unknown_9:
            seq:
              - id: i_distance
                type: u4
          cmd_drop_item:
            seq:
              - id: item0
                type: u2
              - id: item1
                type: u2
              - id: item2
                type: u2
              - id: item3
                type: u2
              - id: item4
                type: u2
              - id: i_to_owner
                type: u4
          cmd_spawn_monster_4:
            seq:
              - id: c_monster
                type: u2
              - id: w_how_many
                type: u2
              - id: i_distance
                type: u4
          cmd_spawn_monster_5:
            seq:
              - id: c_monster
                type: u2
              - id: bt_pos
                type: u1
              - id: i_distance
                type: u4
          cmd_do_skill:
            seq:
              - id: bt_target
                type: u1
              - id: n_skill
                type: u2
              - id: n_motion
                type: u2
          cmd_set_variable_1:
            seq:
              - id: bt_var_idx
                type: u1
              - id: bt_op
                type: u1
              - id: i_value
                type: u4
          cmd_shout_ann_ltb_string:
            seq:
              - id: bt_msg_type
                type: u1
              - id: i_str_id
                type: u4
          cmd_do_trigger:
            seq:
              - id: sz_trigger
                type: sstr
          cmd_zone_1:
            seq:
              - id: n_zone_no
                type: u2
              - id: bt_on_off
                type: u1
          cmd_item:
            seq:
              - id: n_item_num
                type: u2
              - id: n_count
                type: u2
          cmd_set_variable_4:
            seq:
              - id: n_var_idx
                type: u2
              - id: bt_op
                type: u1
              - id: i_value
                type: u4
          cmd_monster_1:
            seq:
              - id: n_monster
                type: u2
              - id: bt_master
                type: u1
          cmd_monster_2:
            seq:
              - id: n_monster
                type: u2
              - id: n_pos
                type: u2
              - id: i_distance
                type: u4
              - id: bt_master
                type: u1
