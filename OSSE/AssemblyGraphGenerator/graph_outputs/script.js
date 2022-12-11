
// https://stackoverflow.com/questions/41514967/yes-no-is-there-a-way-to-improve-mouse-dragging-with-pure-svg-tools/41518545#41518545

var nodes_ls = []
var LinkElement = []

document.querySelectorAll(".node").forEach(function(node){
  nodes_ls.push(node.id)
});

document.querySelectorAll(".edge").forEach(function(edge) {
  var edge_title = edge.getElementsByTagName('title')[0].textContent.split("->")
  var s_num = parseInt(edge_title[0].split('G')[1]).toString()
  var e_num = parseInt(edge_title[1].split('G')[1]).toString()
  var color = edge.getElementsByTagName('polygon')[0].getAttribute('fill')
  var options = {color: color, size: 2, path:'straight'}

  if (s_num != e_num){
    if (parseInt(e_num) != parseInt(s_num) + 1 && parseInt(e_num) != parseInt(s_num) - 1) { 
        var startSocket, endSocket;
        if (color == 'green') {startSocket= 'left'; endSocket= 'left';}
        else {startSocket= 'right'; endSocket= 'right';}
        
        options = {color: color, size: 2, path:'fluid', startSocket:startSocket, endSocket:endSocket}
    }

    LinkElement.push({leftNode : 'node'+s_num, rightNode: 'node'+e_num, options: options})

    edge.remove()
  }
});

var nodes = nodes_ls.reduce(function(nodes, id) {
    var element = document.getElementById(id);
    nodes[id] = {
      element: element,
      draggable: new PlainDraggable(element, {
        onMove: function() {
          nodes[id].lines.forEach(function(line) { line.position(); });
        }
      }),
      lines: []
    };
    return nodes;
}, {}), LinkElement;

LinkElement.forEach(function(link) {
  var leftNode = nodes[link.leftNode],
    rightNode = nodes[link.rightNode],
    line = new LeaderLine(leftNode.element, rightNode.element, link.options);
  leftNode.lines.push(line);
  rightNode.lines.push(line);
});

