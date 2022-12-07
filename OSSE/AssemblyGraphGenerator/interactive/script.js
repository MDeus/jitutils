
// https://stackoverflow.com/questions/41514967/yes-no-is-there-a-way-to-improve-mouse-dragging-with-pure-svg-tools/41518545#41518545

var node_list = document.querySelectorAll(".node")
var edge_list = document.querySelectorAll(".edge")
var map = {};

// create leaderlines
for (var i = 0; i < edge_list.length; i++) {
    var edge_title = edge_list[i].getElementsByTagName('title')[0].textContent.split("->")
    var s_num = parseInt(edge_title[0].split('G')[1]).toString()
    var e_num = parseInt(edge_title[1].split('G')[1]).toString()

    if(s_num == e_num) {continue;}

    // get start and end element of edge
    var startElem = document.getElementById('node'+s_num)
    var endElem = document.getElementById('node'+e_num)

    // get line color
    var color = edge_list[i].getElementsByTagName('polygon')[0].getAttribute('fill')
    var options = {color: color, size: 2, path:'straight'}

    if (parseInt(e_num) != parseInt(s_num) + 1 && parseInt(e_num) != parseInt(s_num) - 1) { 
        var startSocket, endSocket;
        if (color == 'green') {startSocket= 'left'; endSocket= 'left';}
        else {startSocket= 'right'; endSocket= 'right';}
        
        options = {color: color, size: 2, path:'fluid', hoverStyle:{dash:{Animation:true}} ,startSocket:startSocket, endSocket:endSocket}
    }

    // create leader line
    var line = new LeaderLine((startElem), endElem, options)
    
    addValueToList(startElem.id, line)
    addValueToList(endElem.id, line)
    edge_list[i].remove()
}

// draggable = new PlainDraggable(element);
function addValueToList(key, value) {
    //if the list is already created for the "key", then uses it
    //else creates new list for the "key" to store multiple values in it.
    map[key] = map[key] || [];
    map[key].push(value);
}

// for (var i = 0; i < node_list.length; i++) {
//     var id = node_list[i].id
//     var node = node_list[i]
//     // var o = new Draggable(node_list[i], map[id])
//     var lines = map[id]
//     console.log(id)
//     console.log(lines)

//     draggable = new PlainDraggable(node, {
//         onMove: function () {
//             lines.forEach(function(line) { line.position(); });
//             // for (var i = 0; i < lines.length; i++) {lines[i].position()}
//         }
//     });
// }

for (var i = 0; i < node_list.length; i++) {
    var id = node_list[i].id
    var o = new Draggable(node_list[i], map[id])
}

function Draggable(elem, lines) {
    this.target = elem
    this.clickPoint = this.target.ownerSVGElement.createSVGPoint()
    this.lastMove = this.target.ownerSVGElement.createSVGPoint()
    this.currentMove = this.target.ownerSVGElement.createSVGPoint()
    this.target.addEventListener("mousedown", this)
    
    this.handleEvent = function(evt) {
        evt.preventDefault()
        this.clickPoint = globalToLocalCoords(elem, evt.clientX, evt.clientY)
        this.target.classList.add("dragged")
        this.target.setAttribute("pointer-events", "none")
        this.target.ownerSVGElement.addEventListener("mousemove", this.move)
        this.target.ownerSVGElement.addEventListener("mouseup", this.endMove)
    }

    this.move = function(evt) {
        var p = globalToLocalCoords(elem, evt.clientX, evt.clientY)
        this.currentMove.x = this.lastMove.x + (p.x - this.clickPoint.x)
        this.currentMove.y = this.lastMove.y + (p.y - this.clickPoint.y)
        this.target.setAttribute("transform", "translate(" + this.currentMove.x + "," + this.currentMove.y + ")")
        for (var i = 0; i < lines.length; i++) {lines[i].position();}
    }.bind(this)

    this.endMove = function(evt) {
        evt.preventDefault()
        this.lastMove.x = this.currentMove.x
        this.lastMove.y = this.currentMove.y
        this.target.classList.remove("dragged")
        this.target.setAttribute("pointer-events", "all")
        this.target.ownerSVGElement.removeEventListener("mousemove", this.move)
        this.target.ownerSVGElement.removeEventListener("mouseup", this.endMove)
        
    }.bind(this)

    function globalToLocalCoords(elem, x, y) {
        var p = elem.ownerSVGElement.createSVGPoint()
        var m = elem.parentNode.getScreenCTM()
        p.x = x
        p.y = y
        return p.matrixTransform(m.inverse())
    }
}