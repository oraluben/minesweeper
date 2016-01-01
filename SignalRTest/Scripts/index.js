/**
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

            self.$parent.get_init_data(this.mineGroupX, this.mineGroupY);
        });
    },
    events: {
        'init-callback': function (s_json) {
            if (this.ready) return;

            this.ready = true;

            return;
        },
        'click-callback': function (s_json) {
            if (!ready) return;

            // todo: update canvas

            return;
        }
    },
    methods: {
        draw_mine: function (mine_x, mine_y) {
            if (mine_x < 0 || mine_x > 19 || mine_y < 0 || mine_y > 19) return;

            var canvas = this.$el;
            var context = canvas.getContext("2d");

            context.fillStyle = '#F0F0F0';
            context.fillRect(mine_x * 32 + 1, mine_y * 32 + 1, 30, 30);
        },
        click: function (e, left_or_right) {
            var x = e.offsetX % 32, y = e.offsetY % 32;
            if (0 == x || 31 == x || 0 == y || 31 == y) {
                return;  // click the border
            }
            x = Math.floor(e.offsetX / 32);
            y = Math.floor(e.offsetY / 32);

            this.draw_mine(x, y);

            x += this.mineGroupX;
            y += this.mineGroupY;

            var data = JSON.stringify({
                action: "click",
                param: JSON.stringify({
                    type: left_or_right,
                    mine_x: x,
                    mine_y: y,
                }),
            });

            this.$parent.send_to_server();
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
            console.log(s_json);

            var d = JSON.parse(s_json);

            switch (d.type) {
                case "init":
                    console.log(s_json);
                    break;
                case "click":
                    break;
                default:
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
        callback_click_result: function (s_json) {
            // left click & right click
            // is mine or not

            this.$broadcast('click-callback', s_json);

            return;
        },
        callback_init_data: function (s_json) {
            // just update data
            // $watch will be fired in get_init_data

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
                    data: null,
                };
            }
            if (self.data[group_x][group_y]['ready']) {
                self.$broadcast('init-callback', JSON.stringify({
                    group_x: group_x,
                    group_y: group_y,
                    data: self.data[group_x][group_y]['data'],
                }));
                return;
            }

            self.send_to_server(JSON.stringify({
                action: "init",
                param: "",
            }));

            self.data[group_x][group_y]['unwatch'] = this.$watch('data.' + group_x + '.' + group_y + '.data', function (val, oldV) {
                self.data[group_x][group_y]['ready'] = true;

                self.$broadcast('init-callback', JSON.stringify({
                    group_x: group_x,
                    group_y: group_y,
                    data: self.data[group_x][group_y]['data'],
                }));

                self.data[group_x][group_y]['unwatch']();
            });

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