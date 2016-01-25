angular
    .module('BattleBits', ['ngRoute'])
    .controller('LeaderboardController', function ($scope, BattleBitsService, $location) {
        $scope.nextGame = BattleBitsService.nextGame;
        $scope.competition = BattleBitsService.competition;
        $scope.playGame = function() {
            BattleBitsService.playGame();
        };

        BattleBitsService.onCompetitionJoined($scope, function () {
            $scope.competition = BattleBitsService.competition;
        });
        
        BattleBitsService.onGameScheduled($scope, function () {
            $scope.nextGame = BattleBitsService.nextGame;
        });

        BattleBitsService.onGameStarted($scope, function () {
            $location.path('/viewer');
        });

        BattleBitsService.onGameEnded($scope, function () {
            // TODO update last game
        });
    })
    .controller('GamePlayController', function($scope, $interval, BattleBitsService) {
        $scope.guess = 0;
        // TODO load from server
        // NOTE: can one of the HEXs be zero? in that case the printing is sometimes wrong
        $scope.numbers = [21, 59, 192, 204, 153, 99, 66, 12, 199, 200, 222, 250, 21, 59, 192, 204, 153, 99, 66, 12, 199, 200, 222, 250];
        $scope.numbersGuessed = 0;
        $scope.numbersToGuess = $scope.numbers.length;
        $scope.timeLeft = 45;
        $scope.number = $scope.numbers[$scope.numbersGuessed];

        $scope.isBitActive = function(bitPosition) {
            return ($scope.guess & (1 << bitPosition)) !== 0;
        };

        $scope.toggleBit = function(bitPosition) {
            $scope.guess ^= (1 << bitPosition);
        };

        var timer = $interval(function() {
            $scope.timeLeft--;
        }, 1000, 45);

        var numberWatcher = $scope.$watch('guess', function(newValue) {
            if (newValue === $scope.number) {
                BattleBitsService
                    .guess($scope.numbersGuessed, newValue)
                    .then(function(nextNumber) {
                        if (++$scope.numbersGuessed === $scope.numbersToGuess) {
                            $interval.cancel(timer);
                            // TODO Notify that all bits are solved
                        } else {
                            // assign next number
                            $scope.number = $scope.numbers[$scope.numbersGuessed];
                            $scope.guess = 0;
                        }
                    }, function() {

                    });
            }
        });
    })
    .controller('GameDisplayController', function($scope) {

    })
    .service('BattleBitsService', function(competitionId, SignalR, $rootScope) {
        var BatteBitsService = function(competitionId, SignalR) {
            this.competition = null;
            this.nextGame = null;
            this.currentGame = null;
            var that = this;

            var hub = SignalR.BattleBitsHub;
            SignalR.hub.start()
                .done(function() {
                    hub.server.joinCompetition(competitionId)
                        .done(function(competition) {
                            $rootScope.$apply(function() {
                                that.competition = competition;
                                $rootScope.$emit('competition-joined');
                            });
                        }).fail(function(error) {
                            throw error; // TODO Handle error
                        });
                }).fail(function() {
                    // TODO Handle error
                });

            hub.client.gameScheduled = function(game) {
                $rootScope.$apply(function() {
                    that.nextGame = game;
                });
            };

            hub.client.playerJoined = function (player) {
                $rootScope.$apply(function () {
                    if (that.nextGame !== null) {
                        if (that.nextGame.players === null) {
                            that.nextGame.players = [];
                        }
                        that.nextGame.players.push(player);
                    }
                    $rootScope.$emit('player-joined');
                });
            };

            hub.client.playerLeft = function (player) {
                $rootScope.$apply(function () {
                    // TODO
                });
            };

            hub.client.gameStarted = function (game) {
                $rootScope.$apply(function() {
                    that.nextGame = game;
                });
            };

            hub.client.gameEnded = function (game) {
                $rootScope.$apply(function() {
                    that.lastGame = game;
                });
            };

            this.playGame = function() {
                hub.server.joinGame(competitionId)
                    .done(function() {
                        that.enlisted = true;
                    })
                    .fail(function() {
                        // TODO Handle Error
                    });
            };


            this.onCompetitionJoined = function (scope, callback) {
                var unsubscribe = $rootScope.$on('competition-joined', callback);
                scope.$on('$destroy', unsubscribe);
            };
            this.onGameScheduled = function(scope, callback) {
                var unsubscribe = $rootScope.$on('game-scheduled', callback);
                scope.$on('$destroy', unsubscribe);
            };
            this.onGameStarted = function (scope, callback) {
                var unsubscribe = $rootScope.$on('game-started', callback);
                scope.$on('$destroy', unsubscribe);
            };
            this.onPlayerJoined = function (scope, callback) {
                var unsubscribe = $rootScope.$on('player-joined', callback);
                scope.$on('$destroy', unsubscribe);
            };
            this.onPlayerLeft = function (scope, callback) {
                var unsubscribe = $rootScope.$on('player-left', callback);
                scope.$on('$destroy', unsubscribe);
            };
            this.onGameEnded = function (scope, callback) {
                var unsubscribe = $rootScope.$on('game-ended', callback);
                scope.$on('$destroy', unsubscribe);
            };
        };
        return new BatteBitsService(competitionId, SignalR);
    })
    .filter('binary', function () {
        return function(input, length) {
            length = length || 8;
            return _.padStart((input >>> 0).toString(2), length, '0');
        };
    })
    .filter('hex', function() {
        return function(input) {
            return "0x" + input.toString(16);
        };
    });