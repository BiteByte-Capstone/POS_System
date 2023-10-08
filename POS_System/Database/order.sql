use 'pos_db';

CREATE TABLE `order` (
  `order_id` int(11) NOT NULL AUTO_INCREMENT,
  `table_num` int(11) NOT NULL,
  `order_timestamp` datetime NOT NULL,
  `total_amount` varchar(45) NOT NULL,
  PRIMARY KEY (`order_id`)
);