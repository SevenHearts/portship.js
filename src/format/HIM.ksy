meta:
  id: rose_him
  file-extension: him
  endian: le
seq:
  - id: width
    type: u4
  - id: height
    type: u4
  - id: grid_count
    type: u4
  - id: grid_size
    type: f4
  - id: heights
    type: f4
    repeat: expr
    repeat-expr: width * height
  - id: collision_type # currently only known Enum value is "quad"
    type: bstr
  - id: quadentry_count
    type: u4
  - id: quadentries
    type: minmax
    repeat: expr
    repeat-expr: quadentry_count
  - id: quadtree_count
    type: u4
  - id: quadtrees
    type: minmax
    repeat: expr
    repeat-expr: quadtree_count
types:
  bstr:
    seq:
      - id: len
        type: u1
      - id: data
        type: str
        size: len
        encoding: ascii
  minmax:
    seq:
      - id: min_z
        type: f4
      - id: max_z
        type: f4

