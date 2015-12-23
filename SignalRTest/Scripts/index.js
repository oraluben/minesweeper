var mine = Vue.component('mine', {
    template: '<canvas></canvas>',
    data: function () {
        return {
            d0: 1,
        };
    },
    methods: {
        send_to_server: function (s_json) {
            console.warn('init send_to_server called');
        },
        receive_from_server: function (s_json) {
            console.log(s_json);
            // todo
        },
        send_to_server_factory: function (hub) {
            return function (s_json) {
                console.assert(hub != undefined, 'hub is undefined');
                hub.server.sendToServer(s_json);
            };
        },
    },
    compiled: function () {
        var self = this;

        var mine_hub = $.connection.mineHub;
        mine_hub.client.sendToClient = self.receive_from_server;

        $.connection.hub.start().done(function () {
            self.send_to_server = self.send_to_server_factory(mine_hub);
        });

        console.log(self, mine_hub);
        // todo: init visiable mine data
    },
});

(function () {
    new Vue({
        el: '#mine',
    });
})();