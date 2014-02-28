#Paged Panel
PagedPanel is a special layout panel that arranges items in sections called "pages" while maintaining animated fluid layout. Items in the paged panel are arranged in a special way to make sure that pages are kept as intended.

##Panel Parameters
There are numerous parameters that manage the way a paged panel is laid-out.

Parameter | Description
--------  | -----------
Page Size | Along with Padding determines the working area, where the actual page grid is centered.
Padding   | Minimum amount space that should be left empty along the edges of the page.
Cell Size | Size of a single cell of the page grid. Cells have to have uniform size.

All other layout-related parameters are computed from the stuff above. The grid, if it is smaller than the working area (i.e. area after leaving padding empty) then it will be centered in the working area.