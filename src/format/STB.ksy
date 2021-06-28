meta:
  id: rose_stb
  file-extension: stb
  endian: le
seq:
  - id: format
    size: 4
    type: str
    encoding: ascii
  - id: data_offset
    type: u4
  - id: row_count
    type: u4
  - id: column_count
    type: u4
  - id: row_height
    type: u4
  - id: root_column_width
    type: u2
  - id: column_widths
    type: u2
    repeat: expr
    repeat-expr: column_count
  - id: root_column_title
    type: sstr
  - id: column_titles
    type: sstr
    repeat: expr
    repeat-expr: column_count
  - id: root_data
    type: sstr
  - id: first_cell_data
    type: sstr
    repeat: expr
    repeat-expr: row_count - 1
instances:
  data:
    pos: data_offset
    type: data_table
types:
  sstr:
    seq:
      - id: len
        type: u2
      - id: data
        type: str
        size: len
        encoding: ascii
  data_table:
    seq:
      - id: rows
        type: row
        repeat: expr
        repeat-expr: _root.row_count - 1
    types:
      row:
        seq:
          - id: cells
            type: sstr
            repeat: expr
            repeat-expr: _root.column_count - 1
