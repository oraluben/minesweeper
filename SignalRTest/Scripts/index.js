﻿/**
 * Each mine is 30 * 30 px, border is 1 px, totally is 32 * 32 px.
 * One group contains 20 * 20 mines. (640 * 640 px)
 */

// todo: http://vuejs.org/guide/components.html#Custom_Events

Vue.config.debug = true;

var MineGroup = Vue.extend({
    template: '<canvas width="640" height="640" :style="style" v-on:click.stop.prevent="click($event, \'left\')" v-on:contextmenu.stop.prevent="click($event, \'right\')"></canvas>',
    data: function () {
        return {
            ready: false,
            clicked: {},
        };
    },
    props: {
        mineGroupX: Number,
        mineGroupY: Number,
        offsetX: Number,
        offsetY: Number,
    },
    computed: {
        style: function () {
            return {
                position: "absolute",
                left: this.offsetX + "px",
                top: this.offsetY + "px",
            };
        },
    },
    compiled: function () {
        var self = this;
        setTimeout(function () {
            self.init_canvas();

            self.$parent.get_init_data(self.mineGroupX, self.mineGroupY);
        });
    },
    events: {
        'init-callback': function (s_json) {
            if (this.ready) return;

            var data = JSON.parse(s_json);
            if (this.mineGroupX != data.group_x || this.mineGroupY != data.group_y) return; // not target
            data = JSON.parse(data.data);

            //console.log("mine group: " + data.group_x + ", " + data.group_y + " inited with data: " + data.data);

            for (var i = 0; i < 400; i++) {
                this.draw_mine(i % 20, Math.floor(i / 20), data[i]);
            }

            this.ready = true;

            return;
        },
        'click-callback': function (s_json) {
            if (!this.ready) return;

            var self = this;

            var data = JSON.parse(s_json);

            data.filter(function (v_each_mine, i) {
                return v_each_mine.mine_x - self.mineGroupX >= 0 && v_each_mine.mine_x - self.mineGroupX < 20 &&
                    v_each_mine.mine_y - self.mineGroupY >= 0 && v_each_mine.mine_y - self.mineGroupY < 20;
            }).forEach(function (v_each_mine) {
                console.log("draw " + v_each_mine.mine_x + ', ' + v_each_mine.mine_y + ': ' + v_each_mine.val);
                self.draw_mine(v_each_mine.mine_x % 20, v_each_mine.mine_y % 20, v_each_mine.val);
            });

            return;
        }
    },
    methods: {
        draw_mine: function (mine_x, mine_y, action) {
            if (mine_x < 0 || mine_x > 19 || mine_y < 0 || mine_y > 19) return;
            if (0 == action) return;

            var canvas = this.$el;
            var context = canvas.getContext("2d");
            //console.log(action);
            if (action < 0 || undefined === action) {
                // no action given or is mine
                switch (action) {
                    case -1:
                        context.fillStyle = '#FF0000';  // flag
                        break;
                    case -2:
                        context.fillStyle = '#000000';  // boom!
                        break;
                    default:
                        context.fillStyle = '#E6E6E6';  // loading
                        break;
                }
                context.fillRect(mine_x * 32 + 1, mine_y * 32 + 1, 30, 30);
            } else if (action < 10 && action >= 1) {
                context.fillStyle = '#E6E6E6';  // loading
                context.fillRect(mine_x * 32 + 1, mine_y * 32 + 1, 30, 30);
                context.fillStyle = '#000000';  // number
                context.font = "normal bold 20px consolas";
                context.fillText(action - 1, mine_x * 32 + 10, (mine_y + 1) * 32 - 8);
            }
        },
        click: function (e, left_or_right) {
            var x = e.offsetX % 32, y = e.offsetY % 32;
            if (0 == x || 31 == x || 0 == y || 31 == y) {
                return;  // click the border
            }

            x = Math.floor(e.offsetX / 32);
            y = Math.floor(e.offsetY / 32);

            x += this.mineGroupX;
            y += this.mineGroupY;

            console.log(x, y);

            if (this.$parent.get_mine(x, y) != 0) { return; }

            this.draw_mine(x - this.mineGroupX, y - this.mineGroupY);

            var data = JSON.stringify({
                action: "click",
                param: JSON.stringify({
                    data: left_or_right, // "left" or "right"
                    mine_x: x,
                    mine_y: y,
                }),
            });

            this.$parent.send_to_server(data);
        },
        init_canvas: function () {
            var canvas = this.$el;
            var context = canvas.getContext("2d");

            context.fillStyle = '#D0D0D0';
            context.fillRect(0, 0, canvas.width, canvas.height);

            for (var i = 0; i <= 20; i++) {
                this.draw_line(context, 0, i * 32, canvas.width, i * 32);
                this.draw_line(context, i * 32, 0, i * 32, canvas.height);
            }
        },
        draw_line: function (context, begin_x, begin_y, end_x, end_y) {
            context.lineWidth = 1;
            context.beginPath();
            context.moveTo(begin_x, begin_y);
            context.lineTo(end_x, end_y);
            context.stroke();
            context.closePath();
        },
    },
});


Vue.component('mine', {
    template: '<div :style="main_style" draggable="true" @dragstart="drag_start" @drag.stop.prevent="drag_handler">' + // main div
        '<div width="100%" height="100%" :style="transform_style">' + // div use for drag
        '<mine-group v-if="ready" v-for="g in groups" ' +
        ':mine-group-x="g.group_x" ' +
        ':mine-group-y="g.group_y" ' +
        ':offset-x="g.offset_x" ' +
        ':offset-y="g.offset_y" ' +
        'track-by="_id" ' +
        '/>' +
        '</div>' +
        '</div>',
    props: {
        initGroupX: {
            type: Number,
            default: 0,
        },
        initGroupY: {
            type: Number,
            default: 0,
        },
    },
    data: function () {
        return {
            data: {},
            ready: false,
            init: {
                group_x: 0,
                group_y: 0,
            },
            main_style: {
                'z-index': "1",
                position: "absolute",
                overflow: 'hidden',
                height: '100%',
                width: '100%',
            },
            drag: {
                draging: false,
                drag_start_x: 0,
                drag_start_y: 0,
                drag_offset_x: 0,
                drag_offset_y: 0,
            },
        };
    },
    computed: {
        width: function () {
            var width = 0;
            if (this.$el) { width = this.$el.offsetWidth; }
            return width;
        },
        height: function () {
            var height = 0;
            if (this.$el) { res = this.$el.offsetHeight; }
            return height;
        },
        groups: function () {
            // current init position is 0, 0
            var init_group_x = this.initGroupX, init_group_y = this.initGroupY;
            var groups_x = Math.floor(this.width / 640);
            var groups_y = Math.floor(this.height / 640);
            var start_group_x = init_group_x - 1 - Math.round(this.drag.drag_offset_x / 640);
            var start_group_y = init_group_y - 1 - Math.round(this.drag.drag_offset_y / 640);

            var res = []

            for (var group_x = start_group_x; group_x - start_group_x <= groups_x + 4; group_x++) {
                for (var group_y = start_group_y; group_y - start_group_y <= groups_y + 4; group_y++) {
                    res.push({
                        'group_x': group_x * 20,
                        'group_y': group_y * 20,
                        'offset_x': (group_x - init_group_x) * 640,
                        'offset_y': (group_y - init_group_y) * 640,
                    });
                }
            }

            //console.log(res);

            res.forEach(function (v, i) {
                res[i]['_id'] = v.group_x + '_' + v.group_y;
            });
            return res;
        },
        transform_style: function () {
            return {
                transform: 'translate(' + this.drag.drag_offset_x + 'px, ' + this.drag.drag_offset_y + 'px)',
                position: 'absolute',
                top: '0px',
                left: '0px',
            };
        },
    },
    methods: {
        send_to_server: function (s_json) {
            console.warn('init send_to_server called');
        },
        receive_from_server: function (s_json) {
            var d = JSON.parse(s_json);

            switch (d.action) {
                case "init":
                    this.callback_init_data(d.param);
                    break;
                case "click":
                    this.callback_click_result(d.param);
                    break;
                default:
                    console.log("recive unknown message:", s_json);
                    break;
            }
        },
        send_to_server_factory: function (hub) {
            return function (s_json) {
                if (hub == undefined) {
                    console.warn('hub is undefined');
                    return;
                }
                hub.server.sendToServer(s_json);
            };
        },
        get_mine: function (mine_x, mine_y) {
            var offset_x = mine_x % 20, offset_y = mine_y % 20;
            var group_x = mine_x - offset_x, group_y = mine_y - offset_y;

            if (!this.data[group_x][group_y].ready) {
                return -9;
            }
            var data = JSON.parse(this.data[group_x][group_y].data);
            return data[offset_x + offset_y * 20];
        },
        update_mine: function (mine) {
            var offset_x = mine.mine_x % 20, offset_y = mine.mine_y % 20;
            var group_x = mine.mine_x - offset_x, group_y = mine.mine_y - offset_y;

            if (!this.data[group_x][group_y].ready) { return; }
            var data = JSON.parse(this.data[group_x][group_y].data);
            data[offset_x + offset_y * 20] = mine.val;
            this.data[group_x][group_y].data = JSON.stringify(data);
        },
        callback_click_result: function (s_json) {
            var self = this;
            var data = JSON.parse(s_json);

            this.$broadcast('click-callback', JSON.stringify(JSON.parse(data.data).filter(function (each_mine_v) {
                if (self.get_mine(each_mine_v.mine_x, each_mine_v.mine_y) != 0) { return false; }
                self.update_mine(each_mine_v);
                return true;
            })));

            return;
        },
        callback_init_data: function (s_json) {
            var data = JSON.parse(s_json);

            //console.log(data);

            this.data[data.group_x][data.group_y].data = data.data;
            this.data[data.group_x][data.group_y].ready = true;

            this.get_init_data(data.group_x, data.group_y); // recursion to send to mine groups

            return;
        },
        get_init_data: function (group_x, group_y) {
            var self = this;

            if (undefined == self.data[group_x]) {
                self.data[group_x] = {};
            }
            if (undefined == self.data[group_x][group_y]) {
                self.data[group_x][group_y] = {
                    ready: false,
                    data: "",
                };
            }
            if (self.data[group_x][group_y].ready) {
                self.$broadcast('init-callback', JSON.stringify({
                    group_x: group_x,
                    group_y: group_y,
                    data: self.data[group_x][group_y].data,
                }));
                return;
            }

            self.send_to_server(JSON.stringify({
                action: "init",
                param: JSON.stringify({
                    group_x: group_x,
                    group_y: group_y,
                }),
            }));

            return;
        },
        drag_start: function (e) {
            console.log("drag start");
            var x = e.offsetX, y = e.offsetY;

            this.drag.drag_prev_x = x;
            this.drag.drag_prev_y = y;
        },
        drag_handler: function (e) {
            var x = e.offsetX, y = e.offsetY;

            if (0 == x && 0 == y) {
                console.log("drag end");
                return;
            }

            this.drag.drag_offset_x += x - this.drag.drag_prev_x;
            this.drag.drag_offset_y += y - this.drag.drag_prev_y;

            this.drag.drag_prev_x = x;
            this.drag.drag_prev_y = y;
        },
    },
    compiled: function () {
        var self = this;

        var mine_hub = $.connection.mineHub;
        mine_hub.client.sendToClient = self.receive_from_server;

        $.connection.hub.start().done(function () {
            console.assert(mine_hub != undefined, "hub object is null");
            if (mine_hub) {
                self.send_to_server = self.send_to_server_factory(mine_hub);

                console.log("connected to hub");

                self.ready = true;
            } else {
                console.warn("hub connection not started, please refresh the page.");
            }
        });
    },
    components: {
        'mine-group': MineGroup,
    },
});

(function () {
    new Vue({
        el: '#mine',
    });
})();